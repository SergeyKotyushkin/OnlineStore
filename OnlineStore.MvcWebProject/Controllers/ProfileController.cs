using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
using OnlineStore.BuisnessLogic.MappingDtoExtensions;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.App_GlobalResources;
using OnlineStore.MvcWebProject.Models.Profile;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    public class ProfileController : Controller
    {
        public const int PageSize = 3;
        public const int VisiblePagesCount = 7;

        private readonly CultureInfo _threadCulture = Thread.CurrentThread.CurrentCulture;
        private readonly Color _successColor = Color.DarkGreen;
        private readonly Color _failColor = Color.Firebrick;

        private readonly PagerSettings _pagerSettings = new PagerSettings
        {
            PageChangeRoute = new Route {Action = "PageChange", Controller = "Profile"},
            UpdateTargetId = "profileTablePartial"
        };

        private byte[] _defaultImage;

        private readonly IDbOrderHistoryRepository _dbOrderHistoryRepository;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IUserGroup _userGroup;
        private readonly ITableManager<OrderHistoryItemDto, HttpSessionStateBase> _tableManager;
        private readonly IDbPersonRepository _dbPersonRepository;
        private readonly IImageService _imageService;

        public ProfileController(IDbOrderHistoryRepository dbOrderHistoryRepository, IJsonSerializer jsonSerializer,
            IUserGroup userGroup, ITableManager<OrderHistoryItemDto, HttpSessionStateBase> tableManager,
            IDbPersonRepository dbPersonRepository, IImageService imageService)
        {
            _dbOrderHistoryRepository = dbOrderHistoryRepository;
            _jsonSerializer = jsonSerializer;
            _userGroup = userGroup;
            _tableManager = tableManager;
            _dbPersonRepository = dbPersonRepository;
            _imageService = imageService;
        }

        public ActionResult Info(string id)
        {
            var user = _userGroup.GetUser();

            var tableData = GetTableData(id, user.UserName);

            var profile = _dbPersonRepository.GetByName(user.UserName) ??
                          new Person {Login = user.UserName, Name = "", SecondName = ""};

            var imageBytes = Settings.Base64StringBegin +
                             Convert.ToBase64String(profile.Image ?? GetDefaultImageBytes());

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
                    BackVisible = true,
                    SelectedLanguage = _threadCulture.Name
                }
            };

            return View(model);
        }

        [HttpPost]
        public string UploadImage(HttpPostedFileBase file)
        {
            if (file == null) 
                return null;

            var bytes = GetFileBytes(file);

            var image = _imageService.ByteArrayToImage(bytes);
            var size = _imageService.GetSize(image.Size, 200);
            image = new Bitmap(image, size);
            bytes = _imageService.ImageToByteArray(image);

            var url = Settings.Base64StringBegin + Convert.ToBase64String(bytes);

            return JsonConvert.SerializeObject(new {urlToImage = url});
        }

        [HttpPost]
        public string ChangeProfile()
        {
            var user = _userGroup.GetUser();
            var base64Image = Request.Form["imageSrc"].Substring(Settings.Base64StringBegin.Length);

            var person = new Person
            {
                Login = user.UserName,
                Name = Request.Form["firstName"],
                SecondName = Request.Form["secondName"],
                Image = Convert.FromBase64String(base64Image)
            };

            _dbPersonRepository.AddOrUpdate(person);

            return
                JsonConvert.SerializeObject(new {text = Lang.Profile_ProfileChangedSuccessfully, color = _successColor});
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
                    JsonConvert.SerializeObject(new {text = Lang.Profile_PasswordChangedFail, color = _failColor});

            var user = _userGroup.GetUser();
            return newPassword == repeatPassword && user.ChangePassword(oldPassword, newPassword)
                ? JsonConvert.SerializeObject(new {text = Lang.Profile_PasswordChangedSuccessfully, color = _successColor})
                : JsonConvert.SerializeObject(new {text = Lang.Profile_PasswordChangedFail, color = _failColor});
        }

        public PartialViewResult PageChange(int pageIndex)
        {
            var user = _userGroup.GetUser();
            var table = GetTableData(pageIndex.ToString(), user.UserName);

            return PartialView("_profileTable", table);
        }


        private Table<OrderHistoryItemDto> GetTableData(string id, string userName)
        {
            var orderHistoryItemDtoList = GetOrderHistoryItemDtoList(userName);

            var pagesCount = _tableManager.GetPagesCount(orderHistoryItemDtoList.Count, PageSize);

            var newPageIndex = _tableManager.GetNewPageIndex(Session, Settings.Profile_OldPageIndexName, id, pagesCount);

            var data = _tableManager.GetPageData(orderHistoryItemDtoList, newPageIndex, PageSize);

            return new Table<OrderHistoryItemDto>
            {
                Data = data,
                Pager = new Pager
                {
                    PageIndex = newPageIndex,
                    PagesCount = pagesCount,
                    PageSize = PageSize,
                    Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount),
                    PagerSettings = _pagerSettings
                }
            };
        }

        private List<OrderHistoryItemDto> GetOrderHistoryItemDtoList(string userName)
        {
            var history =
                _dbOrderHistoryRepository.GetAll().Where(o => o.PersonName == userName).OrderBy(u => u.Date);

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
                    o => o.ToOrderHistoryItemDto(_threadCulture, Lang.Profile_Quantity, Lang.Profile_Price,
                        Lang.Profile_Total)).ToList();
        }
        
        private byte[] GetDefaultImageBytes()
        {
            return _defaultImage ??
                   (_defaultImage = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/Images/noProfileImage.png")));
        }

        private static byte[] GetFileBytes(HttpPostedFileBase file)
        {
            var ms = new MemoryStream();
            file.InputStream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
