﻿using Carrotware.CMS.Core;
using Carrotware.CMS.DBUpdater;
using Carrotware.CMS.Interface;
using Carrotware.CMS.Mvc.UI.Admin.Models;
using Carrotware.CMS.Security;
using Carrotware.CMS.Security.Models;
using Carrotware.Web.UI.Components;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Controllers {

	[Authorize]
	public class CmsAdminController : Controller {

		protected override void OnAuthorization(AuthorizationContext filterContext) {
			RouteValueDictionary vals = filterContext.RouteData.Values;
			string action = vals["action"].ToString().ToLower();
			string controller = vals["controller"].ToString().ToLower();

			//TODO: non admin or siteman redirect to go here
			if (this.User.Identity.IsAuthenticated) {
				List<string> lstOKNoSiteActions = new List<string>();
				lstOKNoSiteActions.Add("siteinfo");
				lstOKNoSiteActions.Add("filebrowser");
				lstOKNoSiteActions.Add("about");
				lstOKNoSiteActions.Add("userindex");
				lstOKNoSiteActions.Add("roleindex");
				lstOKNoSiteActions.Add("userprofile");
				lstOKNoSiteActions.Add("changepassword");

				try {
					if (action != "databasesetup") {
						if (!lstOKNoSiteActions.Contains(action) && !SiteData.CurretSiteExists) {
							Response.Redirect(SiteFilename.SiteInfoURL);
						}

						if (!SecurityData.IsAuthEditor && action != "notauthorized") {
							Response.Redirect(SiteFilename.NotAuthorizedURL);
						}

						if (!SecurityData.IsAdmin && (action.StartsWith("useradd")
									|| action.StartsWith("useredit")
									|| action.StartsWith("roleadd"))) {
							Response.Redirect(SiteFilename.SiteInfoURL);
						}
					}
				} catch (Exception ex) {
					//assumption is database is probably empty / needs updating, so trigger the under construction view
					if (DatabaseUpdate.SystemNeedsChecking(ex) || DatabaseUpdate.AreCMSTablesIncomplete()) {
						Response.Redirect(SiteFilename.DatabaseSetupURL);
					} else {
						//something bad has gone down, toss back the error
						throw;
					}
				}
			}
		}

		protected ContentPageHelper pageHelper = new ContentPageHelper();
		protected SiteData siteHelper = new SiteData();
		protected WidgetHelper widgetHelper = new WidgetHelper();
		protected CMSConfigHelper cmsHelper = new CMSConfigHelper();

		protected string CurrentDLLVersion {
			get { return SiteData.CurrentDLLVersion; }
		}

		protected Guid SiteID {
			get {
				return SiteData.CurrentSiteID;
			}
		}

		//
		// POST: /Account/LogOff
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff() {
			var manage = new ManageSecurity(this);
			manage.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

			return RedirectToAction("Index");
		}

		public ActionResult UserProfile() {
			ExtendedUserData model = new ExtendedUserData(SecurityData.CurrentUserIdentityName);

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult UserProfile(ExtendedUserData model) {
			if (ModelState.IsValid) {
				var manage = new ManageSecurity(this);

				IdentityResult result = manage.UserManager.SetEmail(model.UserKey, model.Email);
				//result = manage.UserManager.SetPhoneNumber(model.UserKey, model.PhoneNumber);

				ExtendedUserData exUsr = new ExtendedUserData(SecurityData.CurrentUserIdentityName);

				exUsr.UserNickName = model.UserNickName;
				exUsr.FirstName = model.FirstName;
				exUsr.LastName = model.LastName;
				exUsr.UserBio = model.UserBio;

				exUsr.Save();

				if (result.Succeeded) {
					return RedirectToAction("UserProfile");
				}
			}

			return View(model);
		}

		public ActionResult UserEdit(Guid id) {
			UserModel model = new UserModel(id);

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult UserEdit(UserModel model) {
			ExtendedUserData user = model.User;

			if (ModelState.IsValid) {
				var manage = new ManageSecurity(this);

				IdentityResult result = manage.UserManager.SetEmail(user.UserKey, user.Email);
				result = manage.UserManager.SetPhoneNumber(user.UserKey, user.PhoneNumber);

				ExtendedUserData exUsr = new ExtendedUserData(user.UserId);

				exUsr.UserNickName = user.UserNickName;
				exUsr.FirstName = user.FirstName;
				exUsr.LastName = user.LastName;
				exUsr.UserBio = user.UserBio;

				exUsr.Save();

				foreach (var s in model.SiteOptions) {
					if (s.Selected) {
						user.AddToSite(new Guid(s.Value));
					} else {
						user.RemoveFromSite(new Guid(s.Value));
					}
				}

				foreach (var r in model.RoleOptions) {
					if (r.Selected) {
						user.AddToRole(r.Text);
					} else {
						user.RemoveFromRole(r.Text);
					}
				}

				return RedirectToAction("UserEdit", new { @id = user.UserId });
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		public ActionResult UserAdd() {
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult UserAdd(RegisterViewModel model) {
			if (ModelState.IsValid) {
				SecurityData sd = new SecurityData();
				ApplicationUser user = new ApplicationUser { UserName = model.UserName, Email = model.Email };

				IdentityResult result = sd.CreateApplicationUser(user, model.Password);

				if (result.Succeeded) {
					var manage = new ManageSecurity(this);

					user = manage.UserManager.FindByName(model.UserName);

					return RedirectToAction("UserEdit", new { @id = user.Id });
				}

				AddErrors(result);
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public JsonResult AjaxFileUpload(AjaxFileUploadModel model) {
			List<HttpPostedFileBase> files = model.PostedFiles.ToList();

			if (files != null && files.Any()) {
				foreach (HttpPostedFileBase file in files) {
					string res = model.UploadFile(file);

					return Json(res);

					//byte[] data = null;
					//using (Stream inputStream = file.InputStream) {
					//	MemoryStream memoryStream = inputStream as MemoryStream;
					//	if (memoryStream == null) {
					//		memoryStream = new MemoryStream();
					//		inputStream.CopyTo(memoryStream);
					//	}
					//	data = memoryStream.ToArray();
					//}
					//return Json(data);
				}
			}

			return Json(String.Empty);
		}

		[HttpGet]
		public ActionResult FileBrowser(string fldrpath, string useTiny, string returnvalue, string viewmode) {
			FileBrowserModel model = new FileBrowserModel(fldrpath, useTiny, returnvalue, viewmode);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult FileBrowser(FileBrowserModel model) {
			string msg = String.Empty;
			string msgCss = String.Empty;

			if (ModelState.IsValid) {
				model.UploadFile();
				msg = model.FileMsg;
				msgCss = model.FileMsgCss;

				model = new FileBrowserModel(model.QueryPath, model.UseTinyMCE.ToString(), model.ReturnMode.ToString(), model.ViewMode);

				model.FileMsg = msg;
				model.FileMsgCss = msgCss;

				ModelState.Clear();
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult FileBrowserRemove(FileBrowserModel model) {
			model.RemoveFiles();

			ModelState.Clear();

			return RedirectToAction("FileBrowser", new { @fldrpath = model.QueryPath, @useTiny = model.UseTinyMCE, @returnvalue = model.ReturnMode, @viewmode = model.ViewMode });
		}

		[HttpGet]
		public ActionResult SiteDataExport() {
			return View();
		}

		[HttpGet]
		public ActionResult SiteImport(Guid importid) {
			SiteImportNativeModel model = new SiteImportNativeModel(importid);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteImport(SiteImportNativeModel model) {
			ModelState.Clear();

			model.ImportStuff();
			string msg = model.Message;
			bool loaded = true;

			if (!model.HasLoaded) {
				loaded = false;
				ModelState.AddModelError(String.Empty, "No Items Selected For Import");
			}

			model = new SiteImportNativeModel(model.ImportID);
			model.Message = msg;
			model.HasLoaded = loaded;

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpGet]
		public ActionResult SiteImportWP(Guid importid) {
			SiteImportWordpressModel model = new SiteImportWordpressModel(importid);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteImportWP(SiteImportWordpressModel model) {
			ModelState.Clear();
			bool loaded = true;

			if (model.DownloadImages && String.IsNullOrEmpty(model.SelectedFolder)) {
				ModelState.AddModelError("SelectedFolder", "If download images is selected, you must select a target folder.");
			}

			loaded = ModelState.IsValid;

			if (ModelState.IsValid) {
				model.ImportStuff();

				if (!model.HasLoaded) {
					loaded = false;
					ModelState.AddModelError(String.Empty, "No Items Selected For Import");
				}
			}

			var newmodel = new SiteImportWordpressModel(model.ImportID);
			newmodel.Message = model.Message;
			newmodel.HasLoaded = loaded;

			newmodel.PageTemplate = model.PageTemplate;
			newmodel.PostTemplate = model.PostTemplate;
			newmodel.SelectedFolder = model.SelectedFolder;

			newmodel.ImportPages = model.ImportPages;
			newmodel.ImportPosts = model.ImportPosts;
			newmodel.ImportSite = model.ImportSite;

			newmodel.DownloadImages = model.DownloadImages;
			newmodel.FixHtmlBodies = model.FixHtmlBodies;
			newmodel.CreateUsers = model.CreateUsers;
			newmodel.MapUsers = model.MapUsers;

			Helper.HandleErrorDict(ModelState);

			return View(newmodel);
		}

		[HttpGet]
		public ActionResult ContentImport() {
			CMSConfigHelper.CleanUpSerialData();

			FileUpModel model = new FileUpModel();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ContentImport(FileUpModel model) {
			string sXML = String.Empty;

			if (model.PostedFile != null) {
				using (StreamReader sr = new StreamReader(model.PostedFile.InputStream)) {
					sXML = sr.ReadToEnd();
				}
			}

			string sTest = String.Empty;
			if (!string.IsNullOrEmpty(sXML) && sXML.Length > 500) {
				sTest = sXML.Substring(0, 250).ToLower();

				try {
					if (sTest.Contains("<contentpageexport xmlns:xsi=\"http://www.w3.org/2001/xmlschema-instance\" xmlns:xsd=\"http://www.w3.org/2001/xmlschema\">")) {
						ContentPageExport cph = ContentImportExportUtils.DeserializeContentPageExport(sXML);
						ContentImportExportUtils.AssignContentPageExportNewIDs(cph);
						ContentImportExportUtils.SaveSerializedDataExport<ContentPageExport>(cph.NewRootContentID, cph);

						if (cph.ThePage.ContentType == ContentPageType.PageType.ContentEntry) {
							Response.Redirect(SiteFilename.PageAddEditURL + "?importid=" + cph.NewRootContentID.ToString());
						} else {
							Response.Redirect(SiteFilename.BlogPostAddEditURL + "?importid=" + cph.NewRootContentID.ToString());
						}
					}

					if (sTest.Contains("<siteexport xmlns:xsi=\"http://www.w3.org/2001/xmlschema-instance\" xmlns:xsd=\"http://www.w3.org/2001/xmlschema\">")) {
						SiteExport site = ContentImportExportUtils.DeserializeSiteExport(sXML);
						ContentImportExportUtils.AssignSiteExportNewIDs(site);
						ContentImportExportUtils.SaveSerializedDataExport<SiteExport>(site.NewSiteID, site);

						Response.Redirect(SiteFilename.SiteImportURL + "?importid=" + site.NewSiteID.ToString());
					}

					if (sXML.Contains("<channel>") && sXML.Contains("<rss")) {
						int iChnl = sXML.IndexOf("<channel>");
						sTest = sXML.Substring(0, iChnl).ToLower();
					}

					if (sTest.Contains("<!-- this is a wordpress extended rss file generated by wordpress as an export of your")
						&& sTest.Contains("http://purl.org/rss")
						&& sTest.Contains("http://wordpress.org/export")) {
						WordPressSite wps = ContentImportExportUtils.DeserializeWPExport(sXML);
						ContentImportExportUtils.AssignWPExportNewIDs(SiteData.CurrentSite, wps);
						ContentImportExportUtils.SaveSerializedDataExport<WordPressSite>(wps.NewSiteID, wps);

						Response.Redirect(SiteFilename.SiteImportWP_URL + "?importid=" + wps.NewSiteID.ToString());
					}

					ModelState.AddModelError("PostedFile", "File did not appear to match an expected format.");
				} catch (Exception ex) {
					ModelState.AddModelError("PostedFile", ex.ToString());
				}
			} else {
				ModelState.AddModelError("PostedFile", "No file appeared in the upload queue.");
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpGet]
		public ActionResult SiteIndex() {
			PagedData<SiteData> model = new PagedData<SiteData>();
			model.PageSize = -1;
			model.InitOrderBy(x => x.SiteName);

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<SiteData>(SiteData.GetSiteList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteIndex(PagedData<SiteData> model) {
			model.ToggleSort();

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<SiteData>(SiteData.GetSiteList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult ContentSnippetIndex() {
			PagedData<ContentSnippet> model = new PagedData<ContentSnippet>();
			model.PageSize = -1;
			model.InitOrderBy(x => x.ContentSnippetName);

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<ContentSnippet>(SiteData.CurrentSite.GetContentSnippetList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ContentSnippetIndex(PagedData<ContentSnippet> model) {
			model.ToggleSort();

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<ContentSnippet>(SiteData.CurrentSite.GetContentSnippetList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			ModelState.Clear();

			return View(model);
		}

		public ActionResult ChangePassword() {
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model) {
			if (!ModelState.IsValid) {
				return View(model);
			}
			var manage = new ManageSecurity(this);

			var result = await manage.UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

			if (result.Succeeded) {
				var user = await manage.UserManager.FindByIdAsync(User.Identity.GetUserId());
				if (user != null) {
					await manage.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
				}
				return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
			}
			AddErrors(result);
			return View(model);
		}

		[AllowAnonymous]
		public ActionResult DatabaseSetup(string signout) {
			DatabaseSetupModel model = new DatabaseSetupModel();

			DatabaseUpdate du = new DatabaseUpdate();

			if (!String.IsNullOrEmpty(signout)) {
				var manage = new ManageSecurity(this);
				manage.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
			}

			List<DatabaseUpdateMessage> lst = new List<DatabaseUpdateMessage>();
			model.Messages = lst;

			if (DatabaseUpdate.LastSQLError != null) {
				du.HandleResponse(lst, DatabaseUpdate.LastSQLError);
				DatabaseUpdate.LastSQLError = null;
			} else {
				bool bUpdate = true;

				if (!du.DoCMSTablesExist()) {
					bUpdate = false;
				}

				bUpdate = du.DatabaseNeedsUpdate();

				if (bUpdate) {
					DatabaseUpdateStatus status = du.PerformUpdates();
					lst = du.MergeMessages(lst, status.Messages);
				} else {
					DataInfo ver = DatabaseUpdate.GetDbSchemaVersion();
					du.HandleResponse(lst, "Database up-to-date [" + ver.DataValue + "] ");
				}

				bUpdate = du.DatabaseNeedsUpdate();

				if (!bUpdate && DatabaseUpdate.LastSQLError == null) {
					model.CreateUser = !du.UsersExist;
				}
			}

			if (DatabaseUpdate.LastSQLError != null) {
				du.HandleResponse(lst, DatabaseUpdate.LastSQLError);
			}

			model.HasExceptions = lst.Where(x => !String.IsNullOrEmpty(x.ExceptionText)).Any();
			model.Messages = lst;

			using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
				cmsHelper.ResetConfigs();
			}

			return View(model);
		}

		[AllowAnonymous]
		public ActionResult CreateFirstAdmin(string returnUrl) {
			var manage = new ManageSecurity(this);
			manage.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

			DatabaseUpdate du = new DatabaseUpdate();
			if (du.UsersExist) {
				return RedirectToLocal("Index");
			}

			RegisterViewModel model = new RegisterViewModel();

			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult CreateFirstAdmin(RegisterViewModel model) {
			DatabaseUpdate du = new DatabaseUpdate();
			if (du.UsersExist) {
				return RedirectToLocal("Index");
			}

			if (ModelState.IsValid) {
				SecurityData sd = new SecurityData();
				ApplicationUser user = new ApplicationUser { UserName = model.UserName, Email = model.Email };

				IdentityResult result = sd.CreateApplicationUser(user, model.Password);

				if (result.Succeeded) {
					var manage = new ManageSecurity(this);

					user = manage.UserManager.FindByName(model.UserName);

					SecurityData.AddUserToRole(user.UserName, SecurityData.CMSGroup_Admins);
					SecurityData.AddUserToRole(user.UserName, SecurityData.CMSGroup_Users);

					return RedirectToAction("Index");
				}

				AddErrors(result);
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[AllowAnonymous]
		public ActionResult Login(string returnUrl) {
			if (DatabaseUpdate.AreCMSTablesIncomplete()) {
				return RedirectToAction("DatabaseSetup");
			}

			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginViewModel model, string returnUrl) {
			Helper.ForceValidation(ModelState, model);

			if (!ModelState.IsValid) {
				Helper.HandleErrorDict(ModelState);

				return View(model);
			}

			var manage = new ManageSecurity(this);

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, change to shouldLockout: true
			//var user = await manage.UserManager.FindByNameAsync(model.UserName);
			//var res = await manage.UserManager.CheckPasswordAsync(user, model.Password);

			var result = await manage.SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

			switch (result) {
				case SignInStatus.Success:
					return RedirectToLocal(returnUrl);

				case SignInStatus.LockedOut:
					return View("Lockout");

				case SignInStatus.RequiresVerification:
					return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

				case SignInStatus.Failure:
				default:
					ModelState.AddModelError(String.Empty, "Invalid login attempt.");
					return View(model);
			}
		}

		[AllowAnonymous]
		public ActionResult ResetPassword(string code) {
			//return code == null ? View("Error") : View();
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model) {
			if (!ModelState.IsValid) {
				return View(model);
			}

			var manage = new ManageSecurity(this);

			//var user = await UserManager.FindByNameAsync(model.Email);
			var user = await manage.UserManager.FindByEmailAsync(model.Email);
			if (user == null) {
				// Don't reveal that the user does not exist
				return RedirectToAction("ResetPasswordConfirmation");
			}
			//var result = await manage.UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
			SecurityData sd = new SecurityData();
			var result = sd.ResetPassword(user, model.Code, model.Password);
			if (result.Succeeded) {
				return RedirectToAction("ResetPasswordConfirmation");
			}
			AddErrors(result);
			return View();
		}

		[AllowAnonymous]
		public ActionResult ResetPasswordConfirmation() {
			return View();
		}

		[AllowAnonymous]
		public ActionResult ForgotPasswordConfirmation() {
			return View();
		}

		[AllowAnonymous]
		public ActionResult NotAuthorized() {
			return View();
		}

		[AllowAnonymous]
		public ActionResult ForgotPassword() {
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model) {
			var manage = new ManageSecurity(this);

			if (ModelState.IsValid) {
				var user = await manage.UserManager.FindByEmailAsync(model.Email);
				if (user == null) {
					// Don't reveal that the user does not exist or is not confirmed
					return View("ForgotPasswordConfirmation");
				} else {
					SecurityData sd = new SecurityData();
					sd.ResetPassword(model.Email);
					return RedirectToAction("ForgotPasswordConfirmation");
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		public ActionResult Index() {
			if (DatabaseUpdate.AreCMSTablesIncomplete()) {
				return RedirectToAction("DatabaseSetup");
			}

			DashboardInfo model = new DashboardInfo();

			CMSConfigHelper.CleanUpSerialData();

			model.Pages = pageHelper.GetSitePageCount(this.SiteID, ContentPageType.PageType.ContentEntry);
			model.Posts = pageHelper.GetSitePageCount(this.SiteID, ContentPageType.PageType.BlogEntry);

			model.Categories = ContentCategory.GetSiteCount(this.SiteID);
			model.Tags = ContentTag.GetSiteCount(this.SiteID);

			model.Snippets = pageHelper.GetSiteSnippetCount(this.SiteID);

			return View(model);
		}

		public ActionResult About() {
			return View();
		}

		protected void LoadTimeZoneInfo() {
			ViewBag.TimeZoneInfoList = TimeZoneInfo.GetSystemTimeZones();
		}

		protected void LoadDatePattern() {
			Dictionary<string, string> lst = new Dictionary<string, string>();
			lst.Add("yyyy/MM/dd", "YYYY/MM/DD");
			lst.Add("yyyy/M/d", "YYYY/M/D");
			lst.Add("yyyy/MM", "YYYY/MM");
			lst.Add("yyyy/MMMM", "YYYY/MonthName");
			lst.Add("yyyy", "YYYY");

			ViewBag.DatePatternList = lst;
		}

		[HttpGet]
		public ActionResult CategoryAddEdit(Guid? id) {
			ContentCategory model = null;

			if (id.HasValue) {
				model = ContentCategory.Get(id.Value);
			} else {
				model = new ContentCategory();
				model.ContentCategoryID = Guid.Empty;
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CategoryAddEdit(ContentCategory model) {
			Helper.ForceValidation(ModelState, model);

			if (ModelState.IsValid) {
				ContentCategory item = ContentCategory.Get(model.ContentCategoryID);

				if (item == null || (item != null && item.ContentCategoryID == Guid.Empty)) {
					item = new ContentCategory();
					item.SiteID = SiteID;
					item.ContentCategoryID = Guid.NewGuid();
				}

				item.CategorySlug = model.CategorySlug;
				item.CategoryText = model.CategoryText;
				item.IsPublic = model.IsPublic;

				item.Save();

				return RedirectToAction("CategoryAddEdit", new { @id = item.ContentCategoryID });
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CategoryDelete(ContentCategory model) {
			ContentCategory item = ContentCategory.Get(model.ContentCategoryID);
			item.Delete();

			return RedirectToAction("CategoryIndex");
		}

		[HttpGet]
		public ActionResult TagAddEdit(Guid? id) {
			ContentTag model = null;

			if (id.HasValue) {
				model = ContentTag.Get(id.Value);
			} else {
				model = new ContentTag();
				model.ContentTagID = Guid.Empty;
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult TagAddEdit(ContentTag model) {
			Helper.ForceValidation(ModelState, model);

			if (ModelState.IsValid) {
				ContentTag item = ContentTag.Get(model.ContentTagID);

				if (item == null || (item != null && item.ContentTagID == Guid.Empty)) {
					item = new ContentTag();
					item.SiteID = SiteID;
					item.ContentTagID = Guid.NewGuid();
				}

				item.TagSlug = model.TagSlug;
				item.TagText = model.TagText;
				item.IsPublic = model.IsPublic;

				item.Save();

				return RedirectToAction("TagAddEdit", new { @id = item.ContentTagID });
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult TagDelete(ContentTag model) {
			ContentTag item = ContentTag.Get(model.ContentTagID);
			item.Delete();

			return RedirectToAction("TagIndex");
		}

		[HttpGet]
		public ActionResult SiteInfo() {
			LoadTimeZoneInfo();
			LoadDatePattern();

			CMSConfigHelper.CleanUpSerialData();
			SiteData site = null;
			bool bNewSite = !SiteData.CurretSiteExists;

			if (!bNewSite) {
				site = siteHelper.GetCurrentSite();
			} else {
				site = SiteData.InitNewSite(this.SiteID);
			}

			return View(new SiteDataModel(site));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteInfo(SiteDataModel model) {
			Helper.ForceValidation(ModelState, model);

			if (ModelState.IsValid) {
				SiteData site = siteHelper.GetCurrentSite();
				bool bNewSite = false;

				if (site == null) {
					bNewSite = true;
					site = new SiteData();
					site.SiteID = SiteID;
				}

				site = model.Site;

				site.Save();

				//if (sDatePatternOld != ddlDatePattern.SelectedValue || sTimezoneOld != ddlTimeZone.SelectedValue) {
				//	using (ContentPageHelper cph = new ContentPageHelper()) {
				//		cph.BulkBlogFileNameUpdateFromDate(this.SiteID);
				//	}
				//}

				if (model.CreateHomePage) {
					CreateEmptyHome();
				}

				if (bNewSite) {
					return RedirectToAction("Index");
				} else {
					return RedirectToAction("SiteInfo");
				}
			}

			Helper.HandleErrorDict(ModelState);

			LoadTimeZoneInfo();
			LoadDatePattern();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ResetVars() {
			using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
				cmsHelper.ResetConfigs();
			}

			return RedirectToAction("SiteInfo");
		}

		[HttpGet]
		public ActionResult PageAddEdit(Guid? id, Guid? versionid, Guid? importid, string mode) {
			ContentPageModel model = new ContentPageModel();
			model.ImportID = importid;
			model.VersionID = versionid;
			model.Mode = (String.IsNullOrEmpty(mode) || mode.Trim().ToLower() != "raw") ? "html" : "raw";

			ContentPage pageContents = null;
			ViewBag.ContentEditMode = model.Mode;

			if (!id.HasValue && !versionid.HasValue && !importid.HasValue) {
				if (pageContents == null) {
					pageContents = new ContentPage(this.SiteID, ContentPageType.PageType.ContentEntry);
				}

				pageContents.Root_ContentID = Guid.Empty;
			} else {
				if (importid.HasValue) {
					ContentPageExport cpe = ContentImportExportUtils.GetSerializedContentPageExport(importid.Value);
					if (cpe != null) {
						pageContents = cpe.ThePage;
						pageContents.EditDate = SiteData.CurrentSite.Now;
						pageContents.Parent_ContentID = null;
						var rp = pageHelper.GetLatestContentByURL(this.SiteID, false, pageContents.FileName);
						if (rp != null) {
							pageContents.Root_ContentID = rp.Root_ContentID;
							pageContents.ContentID = rp.ContentID;
							pageContents.Parent_ContentID = rp.Parent_ContentID;
							pageContents.NavOrder = rp.NavOrder;
						} else {
							pageContents.Root_ContentID = Guid.Empty;
							pageContents.ContentID = Guid.Empty;
							pageContents.NavOrder = pageHelper.GetSitePageCount(this.SiteID, ContentPageType.PageType.ContentEntry);
						}
					}
				}
				if (versionid.HasValue) {
					pageContents = pageHelper.GetVersion(this.SiteID, versionid.Value);
				}
				if (id.HasValue && pageContents == null) {
					pageContents = pageHelper.FindContentByID(this.SiteID, id.Value);
				}
			}

			model.SetPage(pageContents);

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult PageAddEdit(ContentPageModel model) {
			Helper.ForceValidation(ModelState, model);

			model.Mode = (String.IsNullOrEmpty(model.Mode) || model.Mode.Trim().ToLower() != "raw") ? "html" : "raw";
			ViewBag.ContentEditMode = model.Mode;

			if (ModelState.IsValid) {
				ContentPage page = model.ContentPage;

				var pageContents = pageHelper.FindContentByID(this.SiteID, page.Root_ContentID);
				if (pageContents == null) {
					pageContents = new ContentPage(this.SiteID, ContentPageType.PageType.ContentEntry);
				}

				pageContents.GoLiveDate = page.GoLiveDate;
				pageContents.RetireDate = page.RetireDate;

				pageContents.IsLatestVersion = true;
				pageContents.Thumbnail = page.Thumbnail;

				pageContents.TemplateFile = page.TemplateFile;

				pageContents.TitleBar = page.TitleBar;
				pageContents.NavMenuText = page.NavMenuText;
				pageContents.PageHead = page.PageHead;
				pageContents.FileName = page.FileName;
				pageContents.PageSlug = null;

				pageContents.MetaDescription = page.MetaDescription;
				pageContents.MetaKeyword = page.MetaKeyword;

				pageContents.EditDate = SiteData.CurrentSite.Now;
				pageContents.NavOrder = page.NavOrder;

				pageContents.PageText = page.PageText;
				pageContents.LeftPageText = page.LeftPageText;
				pageContents.RightPageText = page.RightPageText;

				pageContents.PageActive = page.PageActive;
				pageContents.ShowInSiteNav = page.ShowInSiteNav;
				pageContents.ShowInSiteMap = page.ShowInSiteMap;
				pageContents.BlockIndex = page.BlockIndex;

				pageContents.Parent_ContentID = page.Parent_ContentID;

				pageContents.CreditUserId = page.CreditUserId;

				pageContents.EditUserId = SecurityData.CurrentUserGuid;

				pageContents.SavePageEdit();

				if (model.VisitPage) {
					Response.Redirect(pageContents.FileName);
				} else {
					return RedirectToAction("PageAddEdit", new { @id = pageContents.Root_ContentID, @mode = model.Mode });
				}
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpGet]
		public ActionResult BlogPostAddEdit(Guid? id, Guid? versionid, Guid? importid, string mode) {
			ContentPageModel model = new ContentPageModel();
			model.ImportID = importid;
			model.VersionID = versionid;
			model.Mode = (String.IsNullOrEmpty(mode) || mode.Trim().ToLower() != "raw") ? "html" : "raw";

			ContentPage pageContents = null;
			ViewBag.ContentEditMode = model.Mode;

			if (!id.HasValue && !versionid.HasValue && !importid.HasValue) {
				if (pageContents == null) {
					pageContents = new ContentPage(this.SiteID, ContentPageType.PageType.BlogEntry);
				}

				pageContents.Root_ContentID = Guid.Empty;
			} else {
				if (importid.HasValue) {
					ContentPageExport cpe = ContentImportExportUtils.GetSerializedContentPageExport(importid.Value);
					if (cpe != null) {
						pageContents = cpe.ThePage;
						pageContents.EditDate = SiteData.CurrentSite.Now;

						var rp = pageHelper.GetLatestContentByURL(this.SiteID, false, pageContents.FileName);
						if (rp != null) {
							pageContents.Root_ContentID = rp.Root_ContentID;
							pageContents.ContentID = rp.ContentID;
						} else {
							pageContents.Root_ContentID = Guid.Empty;
							pageContents.ContentID = Guid.Empty;
						}
						pageContents.Parent_ContentID = null;
						pageContents.NavOrder = SiteData.BlogSortOrderNumber;
					}
				}
				if (versionid.HasValue) {
					pageContents = pageHelper.GetVersion(this.SiteID, versionid.Value);
				}
				if (id.HasValue && pageContents == null) {
					pageContents = pageHelper.FindContentByID(this.SiteID, id.Value);
				}
			}

			model.SetPage(pageContents);

			return View(model);
		}

		public ActionResult ContentSnippetAddEdit(Guid? id, Guid? versionid, string mode) {
			ViewBag.ContentEditMode = (String.IsNullOrEmpty(mode) || mode.Trim().ToLower() != "raw") ? "html" : "raw";
			ContentSnippet model = null;
			if (id.HasValue) {
				model = ContentSnippet.Get(id.Value);
			}
			if (versionid.HasValue) {
				model = ContentSnippet.Get(versionid.Value);
			}

			if (model == null) {
				model = new ContentSnippet();
				DateTime dtSite = CMSConfigHelper.CalcNearestFiveMinTime(SiteData.CurrentSite.Now);
				model.GoLiveDate = dtSite;
				model.RetireDate = dtSite.AddYears(200);
				model.Root_ContentSnippetID = Guid.Empty;
			}

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult ContentSnippetAddEdit(ContentSnippet model, string mode) {
			ViewBag.ContentEditMode = (String.IsNullOrEmpty(mode) || mode.Trim().ToLower() != "raw") ? "html" : "raw";
			Helper.ForceValidation(ModelState, model);

			if (ModelState.IsValid) {
				ContentSnippet item = ContentSnippet.Get(model.Root_ContentSnippetID);
				if (item == null) {
					item = new ContentSnippet();
					item.Root_ContentSnippetID = Guid.Empty;
					item.SiteID = SiteID;
					item.CreateUserId = SecurityData.CurrentUserGuid;
					item.CreateDate = SiteData.CurrentSite.Now;
				}

				item.GoLiveDate = model.GoLiveDate;
				item.RetireDate = model.RetireDate;
				item.EditUserId = SecurityData.CurrentUserGuid;

				item.ContentSnippetName = model.ContentSnippetName;
				item.ContentSnippetSlug = model.ContentSnippetSlug;
				item.ContentSnippetActive = model.ContentSnippetActive;
				item.ContentBody = model.ContentBody;

				item.Save();

				return RedirectToAction("ContentSnippetAddEdit", new { @id = item.Root_ContentSnippetID, @mode = mode });
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult BlogPostAddEdit(ContentPageModel model) {
			Helper.ForceValidation(ModelState, model);

			model.Mode = (String.IsNullOrEmpty(model.Mode) || model.Mode.Trim().ToLower() != "raw") ? "html" : "raw";
			ViewBag.ContentEditMode = model.Mode;

			if (ModelState.IsValid) {
				ContentPage page = model.ContentPage;

				var pageContents = pageHelper.FindContentByID(this.SiteID, page.Root_ContentID);
				if (pageContents == null) {
					pageContents = new ContentPage(this.SiteID, ContentPageType.PageType.BlogEntry);
				}

				pageContents.GoLiveDate = page.GoLiveDate;
				pageContents.RetireDate = page.RetireDate;

				pageContents.IsLatestVersion = true;
				pageContents.Thumbnail = page.Thumbnail;

				pageContents.TemplateFile = page.TemplateFile;

				pageContents.TitleBar = page.TitleBar;
				pageContents.NavMenuText = page.NavMenuText;
				pageContents.PageHead = page.PageHead;
				pageContents.FileName = page.FileName;
				pageContents.PageSlug = page.PageSlug;

				pageContents.MetaDescription = page.MetaDescription;
				pageContents.MetaKeyword = page.MetaKeyword;

				pageContents.EditDate = SiteData.CurrentSite.Now;
				pageContents.NavOrder = SiteData.BlogSortOrderNumber;

				pageContents.PageText = page.PageText;
				pageContents.LeftPageText = page.LeftPageText;
				pageContents.RightPageText = page.RightPageText;

				pageContents.PageActive = page.PageActive;
				pageContents.ShowInSiteNav = false;
				pageContents.ShowInSiteMap = false;
				pageContents.BlockIndex = page.BlockIndex;

				pageContents.Parent_ContentID = page.Parent_ContentID;

				pageContents.CreditUserId = page.CreditUserId;

				pageContents.EditUserId = SecurityData.CurrentUserGuid;

				List<ContentCategory> lstCat = new List<ContentCategory>();
				List<ContentTag> lstTag = new List<ContentTag>();

				//lstCat = (from cr in model.CategoryOptions
				//		  join l in SiteData.CurrentSite.GetCategoryList() on cr.Value equals l.ContentCategoryID.ToString()
				//		  where cr.Selected
				//		  select l).ToList();
				//lstTag = (from cr in model.TagOptions
				//		  join l in SiteData.CurrentSite.GetTagList() on cr.Value equals l.ContentTagID.ToString()
				//		  where cr.Selected
				//		  select l).ToList();

				lstCat = (from l in SiteData.CurrentSite.GetCategoryList()
						  join cr in model.SelectedCategories on l.ContentCategoryID.ToString().ToLower() equals cr.ToLower()
						  select l).ToList();
				lstTag = (from l in SiteData.CurrentSite.GetTagList()
						  join cr in model.SelectedTags on l.ContentTagID.ToString().ToLower() equals cr.ToLower()
						  select l).ToList();

				pageContents.ContentCategories = lstCat;
				pageContents.ContentTags = lstTag;

				pageContents.SavePageEdit();

				if (model.VisitPage) {
					Response.Redirect(pageContents.FileName);
				} else {
					return RedirectToAction("BlogPostAddEdit", new { @id = pageContents.Root_ContentID, @mode = model.Mode });
				}
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteContentPage(ContentPageModel model) {
			using (ContentPageHelper cph = new ContentPageHelper()) {
				cph.RemoveContent(this.SiteID, model.ContentPage.Root_ContentID);
			}

			if (model.ContentPage.ContentType == ContentPageType.PageType.BlogEntry) {
				return RedirectToAction("BlogPostIndex");
			}

			return RedirectToAction("PageIndex");
		}

		[HttpGet]
		public ActionResult ControlPropertiesEdit(Guid id, Guid pageid, bool? saved) {
			cmsHelper.OverrideKey(pageid);

			Widget w = (from aw in cmsHelper.cmsAdminWidget
						where aw.Root_WidgetID == id
						orderby aw.WidgetOrder, aw.EditDate
						select aw).FirstOrDefault();

			List<ObjectProperty> lstProps = ObjectProperty.GetWidgetProperties(w, pageid);

			WidgetProperties model = new WidgetProperties(w, lstProps);

			if (saved.HasValue && saved.Value) {
				ShowSave();
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ControlPropertiesEdit(WidgetProperties model) {
			cmsHelper.OverrideKey(model.Widget.Root_ContentID);

			Widget w = (from aw in cmsHelper.cmsAdminWidget
						where aw.Root_WidgetID == model.Widget.Root_WidgetID
						orderby aw.WidgetOrder, aw.EditDate
						select aw).FirstOrDefault();

			var props = new List<WidgetProps>();

			foreach (var itm in model.Properties) {
				var p = new WidgetProps();
				p.KeyName = itm.Name;

				switch (itm.FieldMode) {
					case WidgetAttribute.FieldMode.CheckBoxList:
						//multiple selections are possible, since dictionary is used, insure key is unique by appending the ordinal with a | delimeter.
						p = null;
						int CheckedPosition = 0;
						foreach (var v in itm.Options) {
							if (v.Selected) {
								var pp = new WidgetProps();
								pp.KeyName = v.Value + "|" + CheckedPosition.ToString();
								pp.KeyValue = v.Value.ToString();
								props.Add(pp);
								CheckedPosition++;
							}
						}

						break;

					case WidgetAttribute.FieldMode.DropDownList:
					case WidgetAttribute.FieldMode.TextBox:
					case WidgetAttribute.FieldMode.MultiLineTextBox:
					case WidgetAttribute.FieldMode.RichHTMLTextBox:
						p.KeyValue = itm.TextValue;
						break;

					case WidgetAttribute.FieldMode.CheckBox:
						p.KeyValue = itm.CheckBoxState.ToString();
						break;

					default:
						break;
				}

				if (p != null) {
					props.Add(p);
				}
			}

			w.SaveDefaultControlProperties(props);
			w.EditDate = SiteData.CurrentSite.Now;

			List<Widget> lstPageWidgets = cmsHelper.cmsAdminWidget;
			lstPageWidgets.RemoveAll(x => x.Root_WidgetID == w.Root_WidgetID);
			lstPageWidgets.Add(w);
			cmsHelper.cmsAdminWidget = lstPageWidgets;

			ShowSave();

			//return View(model);
			return RedirectToAction("ControlPropertiesEdit", new { @id = w.Root_WidgetID, @pageid = w.Root_ContentID, @saved = true });
		}

		protected void ShowSave() {
			ViewBag.SavedPageAlert = true;
			ViewBag.SavedPageAlertText = "Changes Applied";
		}

		protected void ShowSave(string msg) {
			ViewBag.SavedPageAlert = true;
			ViewBag.SavedPageAlertText = msg;
		}

		[HttpGet]
		public ActionResult SiteMapPop(bool? saved) {
			if (saved.HasValue && saved.Value) {
				ShowSave();
			}

			return SiteMapResult("SiteMapPop");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteMapPop(List<SiteMapOrder> model) {
			return SiteMapResult("SiteMapPop", true, model);
		}

		[HttpGet]
		public ActionResult SiteMap() {
			return SiteMapResult("SiteMap");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteMap(List<SiteMapOrder> model) {
			return SiteMapResult("SiteMap", null, model);
		}

		protected ActionResult SiteMapResult(string viewName) {
			List<SiteMapOrder> model = new List<SiteMapOrder>();

			using (SiteMapOrderHelper orderHelper = new SiteMapOrderHelper()) {
				model = (from c in orderHelper.GetSiteFileList(this.SiteID)
						 orderby c.NavOrder, c.NavMenuText
						 select c).ToList();
			}

			return View(viewName, model);
		}

		protected ActionResult SiteMapResult(string viewName, bool? saved, List<SiteMapOrder> model) {
			using (SiteMapOrderHelper orderHelper = new SiteMapOrderHelper()) {
				orderHelper.UpdateSiteMap(this.SiteID, model);
			}

			return RedirectToAction(viewName, new { @saved = saved });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult FixOrphan() {
			using (SiteMapOrderHelper orderHelper = new SiteMapOrderHelper()) {
				orderHelper.FixOrphanPages(this.SiteID);
			}

			return RedirectToAction("SiteMap");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult FixBlog() {
			pageHelper.FixBlogNavOrder(this.SiteID);

			return RedirectToAction("SiteMap");
		}

		[HttpGet]
		public ActionResult PageAddChild(Guid id, bool? saved) {
			//if (saved.HasValue && saved.Value) {
			//	ShowSave();
			//}

			ContentPageModel model = new ContentPageModel();

			var pageContents = new ContentPage(this.SiteID, ContentPageType.PageType.ContentEntry);
			pageContents.Parent_ContentID = id;
			model.SetPage(pageContents);

			model.VisitPage = false;
			model.ParentID = id;

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult PageAddChild(ContentPageModel model) {
			model.VisitPage = false;

			if (ModelState.IsValid && model.ParentID.HasValue) {
				var pageContents = model.ContentPage;
				pageContents.SiteID = this.SiteID;
				pageContents.ContentType = ContentPageType.PageType.ContentEntry;

				pageContents.SavePageEdit();

				//return RedirectToAction("PageAddChild", new { @id = model.ParentID.Value, @saved = true });
				ShowSave("Page Created");
				model.VisitPage = true;
			}

			Helper.HandleErrorDict(ModelState);
			return View(model);
		}

		[HttpGet]
		public ActionResult PageChildSort(Guid id, bool? saved) {
			if (saved.HasValue && saved.Value) {
				ShowSave();
			}

			PageChildSortModel model = new PageChildSortModel(id);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PageChildSort(PageChildSortModel model) {
			if (model.SortChild) {
				model.SortChildren();

				return View(model);
			} else {
				using (SiteMapOrderHelper orderHelper = new SiteMapOrderHelper()) {
					var lst = orderHelper.ParseChildPageData(model.Sort, model.Root_ContentID);
					orderHelper.UpdateSiteMap(SiteData.CurrentSiteID, lst);
				}

				return RedirectToAction("PageChildSort", new { @id = model.Root_ContentID, @saved = true });
			}
		}

		[HttpGet]
		public ActionResult ContentEdit(Guid id, Guid? widgetid, string field, string mode) {
			ContentSingleModel model = new ContentSingleModel();
			ViewBag.SavedPage = null;

			model.Mode = mode;
			model.Field = field;
			model.PageId = id;
			model.WidgetId = widgetid;

			cmsHelper.OverrideKey(model.PageId);

			if (widgetid.HasValue) {
				Widget pageWidget = (from w in cmsHelper.cmsAdminWidget
									 where w.Root_WidgetID == widgetid.Value
									 select w).FirstOrDefault();

				model.PageText = pageWidget.ControlProperties;
			} else {
				if (cmsHelper.cmsAdminContent != null) {
					var pageContents = cmsHelper.cmsAdminContent;
					switch (field) {
						case "c":
							model.PageText = pageContents.PageText;
							break;

						case "l":
							model.PageText = pageContents.LeftPageText;
							break;

						case "r":
							model.PageText = pageContents.RightPageText;
							break;
					}
				}
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult ContentEdit(ContentSingleModel model) {
			if (ModelState.IsValid) {
				cmsHelper.OverrideKey(model.PageId);

				if (model.WidgetId.HasValue && model.WidgetId.Value != Guid.Empty) {
					List<Widget> lstWidgets = cmsHelper.cmsAdminWidget;

					Widget pageWidget = (from w in lstWidgets
										 where w.Root_WidgetID == model.WidgetId.Value
										 select w).FirstOrDefault();

					pageWidget.ControlProperties = model.PageText;
					pageWidget.WidgetDataID = Guid.NewGuid();
					pageWidget.IsPendingChange = true;

					lstWidgets.RemoveAll(x => x.Root_WidgetID == model.WidgetId.Value);

					lstWidgets.Add(pageWidget);

					cmsHelper.cmsAdminWidget = lstWidgets;
				} else {
					var pageContents = cmsHelper.cmsAdminContent;

					switch (model.Field) {
						case "c":
							pageContents.PageText = model.PageText;
							break;

						case "l":
							pageContents.LeftPageText = model.PageText;
							break;

						case "r":
							pageContents.RightPageText = model.PageText;
							break;
					}

					cmsHelper.cmsAdminContent = pageContents;
				}

				ShowSave();
			}

			return View(model);
		}

		[HttpGet]
		public ActionResult PageIndex() {
			CMSConfigHelper.CleanUpSerialData();
			PageIndexModel model = new PageIndexModel();
			model.SelectedSearch = PageIndexModel.SearchBy.Filtered;

			PagedData<ContentPage> pagedData = new PagedData<ContentPage>();
			pagedData.PageSize = 10;
			pagedData.InitOrderBy(x => x.NavMenuText);
			model.Page = pagedData;

			return PageIndex(model);
		}

		[HttpGet]
		public FileResult ContentExport(Guid? id, Guid? node, bool? comment, string datebegin, string dateend, string exportwhat) {
			Guid guidContentID = id ?? Guid.Empty;
			Guid guidNodeID = node ?? Guid.Empty;
			bool bExportComments = comment ?? false;

			DateTime dateBegin = DateTime.MinValue;
			DateTime dateEnd = DateTime.MaxValue;
			SiteExport.ExportType ExportWhat = SiteExport.ExportType.AllData;

			if (!String.IsNullOrEmpty(datebegin)) {
				dateBegin = Convert.ToDateTime(datebegin).Date;
			}
			if (!String.IsNullOrEmpty(dateend)) {
				dateEnd = Convert.ToDateTime(dateend).Date;
			}
			if (!String.IsNullOrEmpty(exportwhat)) {
				ExportWhat = (SiteExport.ExportType)Enum.Parse(typeof(SiteExport.ExportType), exportwhat, true); ;
			}

			string theXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n";
			string fileName = "export.xml";

			if (guidContentID != Guid.Empty) {
				ContentPageExport content = ContentImportExportUtils.GetExportPage(SiteData.CurrentSiteID, guidContentID);
				theXML = ContentImportExportUtils.GetExportXML<ContentPageExport>(content);

				fileName = String.Format("page_{0}_{1}", content.ThePage.NavMenuText, guidContentID);
			} else {
				SiteExport site = ContentImportExportUtils.GetExportSite(SiteData.CurrentSiteID, ExportWhat);

				site.ThePages.RemoveAll(x => x.ThePage.GoLiveDate < dateBegin);
				site.ThePages.RemoveAll(x => x.ThePage.GoLiveDate > dateEnd);

				if (guidNodeID != Guid.Empty) {
					List<Guid> lst = pageHelper.GetPageHierarchy(SiteData.CurrentSiteID, guidNodeID);
					site.ThePages.RemoveAll(x => !lst.Contains(x.OriginalRootContentID) && x.ThePage.ContentType == ContentPageType.PageType.ContentEntry);
				}

				if (ExportWhat == SiteExport.ExportType.BlogData) {
					site.ThePages.RemoveAll(x => x.ThePage.ContentType == ContentPageType.PageType.ContentEntry);
				}
				if (ExportWhat == SiteExport.ExportType.ContentData) {
					site.ThePages.RemoveAll(x => x.ThePage.ContentType == ContentPageType.PageType.BlogEntry);
				}

				if (bExportComments) {
					site.LoadComments();
				}

				theXML = ContentImportExportUtils.GetExportXML<SiteExport>(site);

				fileName = String.Format("site_{0}_{1}", site.TheSite.SiteName, site.TheSite.SiteID);
			}

			fileName = String.Format("{0}-{1:yyyy-MM-dd}.xml", fileName, SiteData.CurrentSite.Now).Replace(" ", "_");

			return File(Encoding.UTF8.GetBytes(theXML), "application/octet-stream", fileName);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PageIndex(PageIndexModel model) {
			PagedData<ContentPage> pagedData = model.Page;

			pagedData.ToggleSort();
			var srt = pagedData.ParseSort();

			if (model.SelectedSearch == PageIndexModel.SearchBy.AllPages) {
				pagedData.TotalRecords = pageHelper.GetSitePageCount(this.SiteID, ContentPageType.PageType.ContentEntry, false);
				pagedData.DataSource = pageHelper.GetPagedSortedContent(this.SiteID, ContentPageType.PageType.ContentEntry, false, pagedData.PageSize, pagedData.PageNumberZeroIndex, pagedData.OrderBy);
			} else {
				IQueryable<ContentPage> query = null;
				if (!model.ParentPageID.HasValue) {
					query = pageHelper.GetTopNavigation(this.SiteID, false).AsQueryable();
				} else {
					query = pageHelper.GetParentWithChildNavigation(this.SiteID, model.ParentPageID.Value, false).AsQueryable();
				}

				query = query.SortByParm<ContentPage>(srt.SortField, srt.SortDirection);
				pagedData.DataSource = query.ToList();

				pagedData.TotalRecords = pagedData.DataSource.Count();
				pagedData.PageSize = 1 + (pagedData.TotalRecords * 2);
			}

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult CategoryIndex() {
			PagedData<ContentCategory> model = new PagedData<ContentCategory>();
			model.PageSize = -1;
			model.InitOrderBy(x => x.CategoryText);

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<ContentCategory>(SiteData.CurrentSite.GetCategoryList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CategoryIndex(PagedData<ContentCategory> model) {
			model.ToggleSort();

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<ContentCategory>(SiteData.CurrentSite.GetCategoryList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult TagIndex() {
			PagedData<ContentTag> model = new PagedData<ContentTag>();
			model.PageSize = -1;
			model.InitOrderBy(x => x.TagText);

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<ContentTag>(SiteData.CurrentSite.GetTagList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult TagIndex(PagedData<ContentTag> model) {
			model.ToggleSort();

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<ContentTag>(SiteData.CurrentSite.GetTagList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult WidgetList(Guid id, string zone, bool? saved) {
			WidgetListModel model = new WidgetListModel();
			model.Root_ContentID = id;
			model.PlaceholderName = zone;

			if (saved.HasValue && saved.Value) {
				ShowSave();
			}

			return WidgetList(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult WidgetList(WidgetListModel model) {
			List<Widget> widgets = model.Controls;
			cmsHelper.OverrideKey(model.Root_ContentID);

			if (widgets != null && widgets.Any()) {
				var cacheWidget = cmsHelper.cmsAdminWidget;

				foreach (var w in widgets) {
					var ww = (from cw in cacheWidget
							  where cw.Root_WidgetID == w.Root_WidgetID
							  select cw).FirstOrDefault();

					if (w.IsWidgetActive) {
						ww.IsWidgetActive = true;
						ww.IsWidgetPendingDelete = false;
					} else {
						ww.IsWidgetActive = false;
					}

					if (w.IsWidgetPendingDelete) {
						ww.IsWidgetPendingDelete = true;
						ww.IsWidgetActive = false;
					} else {
						ww.IsWidgetPendingDelete = false;
					}

					ww.EditDate = SiteData.CurrentSite.Now;
				}

				cmsHelper.cmsAdminWidget = cacheWidget;

				ShowSave();
			}

			model.Controls = (from aw in cmsHelper.cmsAdminWidget
							  where aw.PlaceholderName.ToLower() == model.PlaceholderName.ToLower() || model.PlaceholderName.ToLower() == "cms-all-placeholder-zones"
							  orderby aw.PlaceholderName ascending, aw.IsWidgetPendingDelete ascending, aw.IsWidgetActive descending, aw.WidgetOrder
							  select aw).ToList();

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult ContentEditHistory() {
			ContentHistoryModel model = new ContentHistoryModel();
			PagedData<EditHistory> history = new PagedData<EditHistory>();
			history.PageSize = 25;
			history.InitOrderBy(x => x.EditDate, false);
			model.Page = history;

			return ContentEditHistory(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ContentEditHistory(ContentHistoryModel model) {
			PagedData<EditHistory> history = model.Page;

			history.ToggleSort();
			var srt = history.ParseSort();

			history.TotalRecords = EditHistory.GetHistoryListCount(SiteData.CurrentSiteID, model.GetLatestOnly, model.SearchDate, model.SelectedUserID);

			if (history.TotalRecords > 0 && (history.PageNumber > history.TotalPages)) {
				history.PageNumber = history.TotalPages;
			}

			history.DataSource = EditHistory.GetHistoryList(history.OrderBy, history.PageNumberZeroIndex, history.PageSize, SiteData.CurrentSiteID, model.GetLatestOnly, model.SearchDate, model.SelectedUserID);

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult PageCommentIndex(Guid? id) {
			CommentIndexModel model = new CommentIndexModel();
			model.PageType = ContentPageType.PageType.ContentEntry;
			model.Root_ContentID = id;

			return CommentIndex(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PageCommentIndex(CommentIndexModel model) {
			model.PageType = ContentPageType.PageType.ContentEntry;

			return CommentIndex(model);
		}

		[HttpGet]
		public ActionResult PostCommentIndex(Guid? id) {
			CommentIndexModel model = new CommentIndexModel();
			model.PageType = ContentPageType.PageType.BlogEntry;
			model.Root_ContentID = id;

			return CommentIndex(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PostCommentIndex(CommentIndexModel model) {
			model.PageType = ContentPageType.PageType.BlogEntry;

			return CommentIndex(model);
		}

		protected ActionResult CommentIndex(CommentIndexModel model) {
			PagedData<PostComment> pagedData = model.Comments;

			pagedData.ToggleSort();

			if (!model.Root_ContentID.HasValue) {
				pagedData.TotalRecords = PostComment.GetCommentCountBySiteAndType(SiteData.CurrentSiteID, model.PageType);
			} else {
				pagedData.TotalRecords = PostComment.GetCommentCountByContent(model.Root_ContentID.Value, false);
			}

			if (!model.Root_ContentID.HasValue) {
				pagedData.DataSource = PostComment.GetCommentsBySitePageNumber(SiteData.CurrentSiteID, pagedData.PageNumberZeroIndex, pagedData.PageSize, pagedData.OrderBy, model.PageType);
			} else {
				pagedData.DataSource = PostComment.GetCommentsByContentPageNumber(model.Root_ContentID.Value, pagedData.PageNumberZeroIndex, pagedData.PageSize, pagedData.OrderBy, false);
			}

			ModelState.Clear();

			return View("CommentIndex", model);
		}

		[HttpGet]
		public ActionResult BlogPostIndex() {
			CMSConfigHelper.CleanUpSerialData();
			PostIndexModel model = new PostIndexModel();
			model.SelectedSearch = PageIndexModel.SearchBy.AllPages;

			PagedData<ContentPage> pagedData = new PagedData<ContentPage>();
			pagedData.PageSize = 10;
			pagedData.InitOrderBy(x => x.GoLiveDate, false);
			model.Page = pagedData;

			return BlogPostIndex(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult BlogPostIndex(PostIndexModel model) {
			PagedData<ContentPage> pagedData = model.Page;

			pagedData.ToggleSort();
			var srt = pagedData.ParseSort();

			if (model.SelectedSearch == PageIndexModel.SearchBy.AllPages) {
				pagedData.TotalRecords = pageHelper.GetSitePageCount(this.SiteID, ContentPageType.PageType.BlogEntry, false);
				pagedData.DataSource = pageHelper.GetPagedSortedContent(this.SiteID, ContentPageType.PageType.BlogEntry, false, pagedData.PageSize, pagedData.PageNumberZeroIndex, pagedData.OrderBy);
			} else {
				IQueryable<ContentPage> query = pageHelper.GetPostsByDateRange(this.SiteID, model.SearchDate, model.SelectedRange, false).AsQueryable();
				query = query.SortByParm<ContentPage>(srt.SortField, srt.SortDirection);

				pagedData.DataSource = query.ToList();

				pagedData.TotalRecords = pagedData.DataSource.Count();
				pagedData.PageSize = 1 + (pagedData.TotalRecords * 2);
			}

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult UserIndex() {
			PagedData<ExtendedUserData> model = new PagedData<ExtendedUserData>();
			model.PageSize = -1;
			model.InitOrderBy(x => x.UserName);

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<ExtendedUserData>(ExtendedUserData.GetUserList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult UserIndex(PagedData<ExtendedUserData> model) {
			model.ToggleSort();

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<ExtendedUserData>(ExtendedUserData.GetUserList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult RoleIndex() {
			PagedData<UserRole> model = new PagedData<UserRole>();
			model.PageSize = -1;
			model.InitOrderBy(x => x.RoleName);

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<UserRole>(SecurityData.GetRoleList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult RoleIndex(PagedData<UserRole> model) {
			model.ToggleSort();

			var srt = model.ParseSort();
			var query = ReflectionUtilities.SortByParm<UserRole>(SecurityData.GetRoleList(), srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult RoleAddEdit(string id) {
			RoleModel model = null;

			if (!String.IsNullOrEmpty(id)) {
				model = new RoleModel(id);
			} else {
				model = new RoleModel();
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult RoleAddEdit(RoleModel model) {
			Helper.ForceValidation(ModelState, model);

			if (ModelState.IsValid) {
				UserRole role = model.Role;
				UserRole item = SecurityData.FindRole(role.RoleName);

				if (item == null) {
					item = SecurityData.FindRoleByID(role.RoleId);
				}

				if (item == null) {
					item = new UserRole();
					item.RoleId = role.RoleId;
				}

				item.RoleName = role.RoleName.Trim();

				item.Save();

				return RedirectToAction("RoleAddEdit", new { @id = item.RoleId });
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult RoleRemoveUsers(RoleModel model) {
			UserRole role = model.Role;

			if (ModelState.IsValid) {
				List<UserModel> usrs = model.Users.Where(x => x.Selected).ToList();

				foreach (var u in usrs) {
					SecurityData.RemoveUserFromRole(u.User.UserName, role.RoleName);
				}

				return RedirectToAction("RoleAddEdit", new { @id = role.RoleId });
			}

			Helper.HandleErrorDict(ModelState);

			return View("RoleAddEdit", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult RoleAddUser(RoleModel model) {
			if (String.IsNullOrEmpty(model.NewUserId)) {
				ModelState.AddModelError("NewUserId", "The New User field is required.");
			}

			Helper.ForceValidation(ModelState, model);
			UserRole role = model.Role;

			if (ModelState.IsValid) {
				if (!String.IsNullOrEmpty(model.NewUserId)) {
					SecurityData.AddUserToRole(new Guid(model.NewUserId), role.RoleName);
				}

				return RedirectToAction("RoleAddEdit", new { @id = role.RoleId });
			}

			Helper.HandleErrorDict(ModelState);

			model.LoadUsers();

			return View("RoleAddEdit", model);
		}

		protected void LoadSiteTemplateList() {
			ViewBag.SiteTemplateList = cmsHelper.Templates;
		}

		[HttpGet]
		public ActionResult SiteTemplateUpdate() {
			SiteTemplateUpdateModel model = new SiteTemplateUpdateModel();
			LoadSiteTemplateList();

			ContentPage pageHome = pageHelper.FindHome(this.SiteID, true);
			ContentPage pageIndex = null;

			if (pageHome != null) {
				model.HomePage = pageHome.TemplateFile.ToLower();
				model.HomePageTitle = pageHome.NavMenuText;
				model.HomePageLink = pageHome.FileName;

				if (SiteData.CurrentSite.Blog_Root_ContentID.HasValue) {
					pageIndex = pageHelper.FindContentByID(this.SiteID, SiteData.CurrentSite.Blog_Root_ContentID.Value);
					model.IndexPageID = SiteData.CurrentSite.Blog_Root_ContentID.Value;
					model.IndexPage = pageIndex.TemplateFile.ToLower();
				}
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteTemplateUpdate(SiteTemplateUpdateModel model) {
			LoadSiteTemplateList();

			ContentPage pageHome = pageHelper.FindHome(this.SiteID, true);
			ContentPage pageIndex = null;

			SiteData.CurrentSite.Blog_Root_ContentID = model.IndexPageID;
			if (model.IndexPageID.HasValue) {
				pageIndex = pageHelper.FindContentByID(this.SiteID, model.IndexPageID.Value);
			}
			SiteData.CurrentSite.Save();

			if (!String.IsNullOrEmpty(model.BlogPages)) {
				pageHelper.UpdateAllBlogTemplates(this.SiteID, model.BlogPages);
			}
			if (!String.IsNullOrEmpty(model.AllPages)) {
				pageHelper.UpdateAllPageTemplates(this.SiteID, model.AllPages);
			}

			if (!String.IsNullOrEmpty(model.TopPages)) {
				pageHelper.UpdateTopPageTemplates(this.SiteID, model.TopPages);
			}
			if (!String.IsNullOrEmpty(model.SubPages)) {
				pageHelper.UpdateSubPageTemplates(this.SiteID, model.SubPages);
			}

			if (pageHome != null && !String.IsNullOrEmpty(model.HomePage)) {
				pageHome.TemplateFile = model.HomePage;
				pageHome.ApplyTemplate();
			}
			if (pageIndex != null && !String.IsNullOrEmpty(model.IndexPage)) {
				pageIndex.TemplateFile = model.IndexPage;
				pageIndex.ApplyTemplate();
			}

			if (!String.IsNullOrEmpty(model.AllContent)) {
				pageHelper.UpdateAllContentTemplates(this.SiteID, model.AllContent);
			}

			return RedirectToAction("SiteTemplateUpdate");
		}

		private void CreateEmptyHome() {
			DateTime dtSite = CMSConfigHelper.CalcNearestFiveMinTime(SiteData.CurrentSite.Now);

			ContentPage pageContents = new ContentPage {
				SiteID = SiteID,
				Root_ContentID = Guid.NewGuid(),
				ContentID = Guid.NewGuid(),
				EditDate = SiteData.CurrentSite.Now,
				CreateUserId = SecurityData.CurrentUserGuid,
				CreateDate = SiteData.CurrentSite.Now,
				GoLiveDate = dtSite.AddMinutes(-5),
				RetireDate = dtSite.AddYears(200),
				TitleBar = "Home",
				NavMenuText = "Home",
				PageHead = "Home",
				FileName = "/home",
				PageText = SiteData.StarterHomePageSample,
				LeftPageText = String.Empty,
				RightPageText = String.Empty,
				NavOrder = 0,
				IsLatestVersion = true,
				PageActive = true,
				ShowInSiteNav = true,
				ShowInSiteMap = true,
				BlockIndex = false,
				EditUserId = SecurityData.CurrentUserGuid,
				ContentType = ContentPageType.PageType.ContentEntry,
				TemplateFile = SiteData.DefaultTemplateFilename
			};

			pageContents.SavePageEdit();
		}

		public ActionResult TemplatePreview() {
			PagePayload _page = new PagePayload();
			_page.ThePage = ContentPageHelper.GetSamplerView();
			_page.ThePage.TemplateFile = SiteData.PreviewTemplateFile;

			_page.TheSite = SiteData.CurrentSite;
			_page.TheWidgets = new List<Widget>();

			_page.HandleTemplatePath(this);

			this.ViewData[PagePayload.ViewDataKey] = _page;

			return View(SiteData.PreviewTemplateFile);
		}

		private void AddErrors(IdentityResult result) {
			foreach (var error in result.Errors) {
				ModelState.AddModelError("", error);
			}
		}

		private ActionResult RedirectToLocal(string returnUrl) {
			if (Url.IsLocalUrl(returnUrl)) {
				return Redirect(returnUrl);
			}

			return RedirectToAction("Index");
		}

		public ActionResult TextWidgetIndex() {
			List<CMSTextWidgetPicker> model = new List<CMSTextWidgetPicker>();

			using (CMSConfigHelper cfg = new CMSConfigHelper()) {
				model = cfg.GetAllWidgetSettings(this.SiteID);
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult TextWidgetIndex(List<CMSTextWidgetPicker> model) {
			foreach (CMSTextWidgetPicker w in model) {
				TextWidget ww = new TextWidget();
				ww.SiteID = this.SiteID;
				ww.TextWidgetID = w.TextWidgetPickerID;
				ww.TextWidgetAssembly = w.AssemblyString;

				ww.ProcessBody = w.ProcessBody;
				ww.ProcessPlainText = w.ProcessPlainText;
				ww.ProcessHTMLText = w.ProcessHTMLText;
				ww.ProcessComment = w.ProcessComment;
				ww.ProcessSnippet = w.ProcessSnippet;

				if (ww.ProcessBody || ww.ProcessPlainText || ww.ProcessHTMLText || ww.ProcessComment || ww.ProcessSnippet) {
					ww.Save();
				} else {
					ww.Delete();
				}
			}

			if (SiteData.CurretSiteExists) {
				SiteData.CurrentSite.LoadTextWidgets();
			}

			return RedirectToAction("TextWidgetIndex");
		}

		public ActionResult ModuleIndex() {
			return View();
		}

		protected override void Dispose(bool disposing) {
			if (pageHelper != null) {
				pageHelper.Dispose();
			}
			if (widgetHelper != null) {
				widgetHelper.Dispose();
			}
			if (cmsHelper != null) {
				cmsHelper.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}