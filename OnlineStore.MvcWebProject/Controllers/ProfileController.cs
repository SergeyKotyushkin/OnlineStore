using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Database.Models.Dto;
using OnlineStore.BuisnessLogic.ImageService.Contracts;
using OnlineStore.BuisnessLogic.JsonSerialize.Contracts;
using OnlineStore.BuisnessLogic.MappingDto;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.Models.Profile;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    public class ProfileController : Controller
    {
        public const int PageSize = 3;
        public const int VisiblePagesCount = 7;
        private const string OldPageIndexName = "CurrentPageIndexOH";
        private const string Base64StringBegin = "data:image/jpg;base64,";

        private byte[] _defaultImage;

        private readonly IDbOrderHistoryRepository _dbOrderHistoryRepository;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IUserGroup _userGroup;
        private readonly ITableManager<OrderHistoryItemDto> _tableManager;
        private readonly IStorageRepository<HttpSessionStateBase> _storageSessionRepository;
        private readonly IDbPersonRepository _dbPersonRepository;
        private readonly IImageService _imageService;

        public ProfileController(IDbOrderHistoryRepository dbOrderHistoryRepository, IJsonSerializer jsonSerializer,
            IUserGroup userGroup, ITableManager<OrderHistoryItemDto> tableManager,
            IStorageRepository<HttpSessionStateBase> storageSessionRepository, IDbPersonRepository dbPersonRepository,
            IImageService imageService)
        {
            _dbOrderHistoryRepository = dbOrderHistoryRepository;
            _jsonSerializer = jsonSerializer;
            _userGroup = userGroup;
            _tableManager = tableManager;
            _storageSessionRepository = storageSessionRepository;
            _dbPersonRepository = dbPersonRepository;
            _imageService = imageService;
        }

        public ActionResult UserInfo(string id)
        {
            var user = _userGroup.GetUser();

            var tableData = GetTableData(id, user.UserName);

            var profile = _dbPersonRepository.GetAll().FirstOrDefault(p => p.Login == user.UserName) ?? new Person
            {
                Login = user.UserName,
                Name = "",
                SecondName = ""
            };

            var imageBytes = Base64StringBegin + Convert.ToBase64String(profile.Image ?? GetDefaultImageBytes());

            var model = new ProfileModel
            {
                UserProfile = new PersonDto
                {
                    ImagePath = imageBytes,
                    FirstName = profile.Name,
                    SecondName = profile.SecondName,
                    Login = user.UserName
                },
                TableData = tableData,
                Settings = new MainLayoutSettings
                {
                    Title = Lang.Profile_Title,
                    LogoutVisible = true,
                    RouteBack = new Route {Action = "ProductList", Controller = "ProductCatalog"},
                    SelectedLanguage = Thread.CurrentThread.CurrentCulture.Name
                }
            };

            return View(model);
        }

        [HttpPost]
        public string UploadImage(HttpPostedFileBase file)
        {
            if (file == null) return null;

            var ms = new MemoryStream();
            file.InputStream.CopyTo(ms);
            var bytes = ms.ToArray();

            var image = _imageService.ByteArrayToImage(bytes);
            var size = _imageService.GetSize(image.Size, 200);
            image = new Bitmap(image, size);
            bytes = _imageService.ImageToByteArray(image);

            var uri = Base64StringBegin + Convert.ToBase64String(bytes);

            return JsonConvert.SerializeObject(new {urlToImage = uri});
        }

        [HttpPost]
        public string ChangeProfile()
        {
            var user = _userGroup.GetUser();
            var base64Image = Request.Form["imageSrc"].Substring(Base64StringBegin.Length);

            var person = new Person
            {
                Login = user.UserName,
                Name = Request.Form["firstName"],
                SecondName = Request.Form["secondName"],
                Image = Convert.FromBase64String(base64Image)
            };

            _dbPersonRepository.AddOrUpdate(person);

            return JsonConvert.SerializeObject(new {text = Lang.Profile_ProfileChangedSuccessfully, color = "DarkGreen"});
        }

        [HttpPost]
        public string ChangePassword()
        {
            var oldPassword = Request.Form["oldPassword"];
            var newPassword = Request.Form["newPassword"];
            var repeatPassword = Request.Form["repeatPassword"];

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) ||
                string.IsNullOrEmpty(repeatPassword))
                return
                    JsonConvert.SerializeObject(
                        new { text = Lang.Profile_PasswordChangedFail, color = "#dc143c" });

            var user = _userGroup.GetUser();
            return newPassword == repeatPassword && user.ChangePassword(oldPassword, newPassword)
                ? JsonConvert.SerializeObject(new { text = Lang.Profile_PasswordChangedSuccessfully, color = "DarkGreen" })
                : JsonConvert.SerializeObject(new { text = Lang.Profile_PasswordChangedFail, color = "#dc143c" });
        }

        public PartialViewResult PageChange(int pageIndex)
        {
            var user = _userGroup.GetUser();
            var table = GetTableData(pageIndex.ToString(), user.UserName);

            return PartialView("_profileTable", table);
        }


        private OrderHistoryItemDto[] GetOrderToGridList(string userName)
        {
            var history = _dbOrderHistoryRepository.GetAll().Where(u => u.PersonName == userName).OrderBy(u => u.Date).ToList();

            var number = 1;
            var ordersFromHistory = (from h in history
                                     let productOrder = _jsonSerializer.Deserialize<ProductOrder[]>(h.Order)
                                     select new OrderHistoryItem
                                     {
                                         Number = number++,
                                         Email = h.PersonEmail,
                                         Date = h.Date,
                                         ProductOrder = productOrder,
                                         Total = h.Total,
                                         CultureName = h.Culture
                                     }).ToArray();

            return
                ordersFromHistory.Select(
                    o =>
                        OrderHistoryItemDtoMapping.ToDto(o, Thread.CurrentThread.CurrentCulture, Lang.Profile_Quantity,
                            Lang.Profile_Price, Lang.Profile_Total)).ToArray();
        }

        private static int GetPagesCount(int itemsCount)
        {
            return (int)Math.Ceiling((double)itemsCount / PageSize);
        }

        private int GetNewPageIndex(string id, int pagesCount)
        {
            var oldPageIndex = (int)(_storageSessionRepository.Get(Session, OldPageIndexName) ?? 1);
            var newPageIndex = _tableManager.GetPageIndex(id, oldPageIndex, pagesCount);
            _storageSessionRepository.Set(Session, OldPageIndexName, newPageIndex);

            return newPageIndex;
        }
        
        private Table<OrderHistoryItemDto> GetTableData(string id, string userName)
        {
            var orderHistoryItemDtos = GetOrderToGridList(userName);

            var pagesCount = GetPagesCount(orderHistoryItemDtos.Length);

            var newPageIndex = GetNewPageIndex(id, pagesCount);

            var data = _tableManager.GetPageData(orderHistoryItemDtos.ToList(), newPageIndex, PageSize).ToArray();

            return new Table<OrderHistoryItemDto>
            {
                Data = data,
                Pager = new Pager
                {
                    PageIndex = newPageIndex,
                    PagesCount = pagesCount,
                    PageSize = PageSize,
                    PagerVisible = true,
                    Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount),
                    PagerSettings =
                        new PagerSettings
                        {
                            PageChangeRoute = new Route { Action = "PageChange", Controller = "Profile" },
                            UpdateTargetId = "profileTablePartial"
                        }
                }
            };
        }

        private byte[] GetDefaultImageBytes()
        {
            if (_defaultImage != null) return _defaultImage;

            _defaultImage = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/Images/noProfileImage.png"));
            return _defaultImage;
        }
    }
}
