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
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Controllers {

	[CmsAuthorize]
	public class CmsAdminController : Controller {

		protected override void OnAuthorization(AuthorizationContext filterContext) {
			base.OnAuthorization(filterContext);

			RouteValueDictionary vals = filterContext.RouteData.Values;
			string action = vals["action"].ToString().ToLowerInvariant();
			string controller = vals["controller"].ToString().ToLowerInvariant();

			if (this.User.Identity.IsAuthenticated) {
				List<string> lstOKNoSiteActions = new List<string>();
				lstOKNoSiteActions.Add("siteinfo");
				lstOKNoSiteActions.Add("filebrowser");
				lstOKNoSiteActions.Add("about");
				lstOKNoSiteActions.Add("userindex");
				lstOKNoSiteActions.Add("roleindex");
				lstOKNoSiteActions.Add("userprofile");
				lstOKNoSiteActions.Add("changepassword");
				lstOKNoSiteActions.Add("login");
				lstOKNoSiteActions.Add("logoff");

				List<string> lstInitSiteActions = new List<string>();
				lstInitSiteActions.Add("login");
				lstInitSiteActions.Add("forgotpassword");
				lstInitSiteActions.Add("createfirstadmin");
				lstInitSiteActions.Add("databasesetup");

				try {
					if (!lstInitSiteActions.Contains(action)) {
						if (!lstOKNoSiteActions.Contains(action) && !SiteData.CurretSiteExists) {
							filterContext.Result = new RedirectResult(SiteFilename.SiteInfoURL);
							return;
						}

						if (DatabaseUpdate.TablesIncomplete) {
							filterContext.Result = new RedirectResult(SiteFilename.DatabaseSetupURL);
							return;
						}
					}
				} catch (Exception ex) {
					//assumption is database is probably empty / needs updating, so trigger the under construction view
					if (DatabaseUpdate.SystemNeedsChecking(ex) || DatabaseUpdate.AreCMSTablesIncomplete()) {
						filterContext.Result = new RedirectResult(SiteFilename.DatabaseSetupURL);
						return;
					} else {
						//something bad has gone down, toss back the error
						throw;
					}
				}
			}
		}

		protected SecurityHelper securityHelper = new SecurityHelper();
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
			SignOut();

			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult UserProfile() {
			ExtendedUserData model = new ExtendedUserData(SecurityData.CurrentUserIdentityName);

			ShowSaved("Profile Updated");

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult UserProfile(ExtendedUserData model) {
			if (ModelState.IsValid) {
				IdentityResult result = securityHelper.UserManager.SetEmail(model.UserKey, model.Email);

				ExtendedUserData exUsr = new ExtendedUserData(SecurityData.CurrentUserIdentityName);

				exUsr.UserNickName = model.UserNickName;
				exUsr.FirstName = model.FirstName;
				exUsr.LastName = model.LastName;
				exUsr.UserBio = model.UserBio;

				exUsr.Save();

				if (result.Succeeded) {
					SetSaved();
					return RedirectToAction("UserProfile");
				}
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpGet]
		[CmsAdminAuthorize]
		public ActionResult UserEdit(Guid id) {
			UserModel model = new UserModel(id);

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		[CmsAdminAuthorize]
		public ActionResult UserEdit(UserModel model) {
			ExtendedUserData userExt = model.User;

			if (ModelState.IsValid) {
				var user = securityHelper.UserManager.FindByName(model.User.UserName);

				IdentityResult result = securityHelper.UserManager.SetEmail(userExt.UserKey, userExt.Email);
				result = securityHelper.UserManager.SetPhoneNumber(userExt.UserKey, userExt.PhoneNumber);

				if (userExt.LockoutEndDateUtc.HasValue) {
					//DateTime utcDateTime = DateTime.SpecifyKind(userExt.LockoutEndDateUtc.Value, DateTimeKind.Utc);
					//DateTimeOffset utcOffset = utcDateTime;
					//result = manage.UserManager.SetLockoutEnabled(userExt.UserKey, true);
					//result = manage.UserManager.SetLockoutEndDate(userExt.UserKey, utcOffset);
					if (!user.LockoutEndDateUtc.HasValue) {
						// set lockout
						user.LockoutEndDateUtc = userExt.LockoutEndDateUtc.Value;
						user.AccessFailedCount = 20;
						securityHelper.UserManager.Update(user);
					}
				} else {
					if (user.LockoutEndDateUtc.HasValue) {
						// unset lockout
						user.LockoutEndDateUtc = null;
						user.AccessFailedCount = 0;
						securityHelper.UserManager.Update(user);
					}
				}

				ExtendedUserData exUsr = new ExtendedUserData(userExt.UserId);

				exUsr.UserNickName = userExt.UserNickName;
				exUsr.FirstName = userExt.FirstName;
				exUsr.LastName = userExt.LastName;
				exUsr.UserBio = userExt.UserBio;

				exUsr.Save();

				model.SaveOptions();

				return RedirectToAction("UserEdit", new { @id = userExt.UserId });
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpGet]
		[CmsAdminAuthorize]
		public ActionResult UserAdd() {
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[CmsAdminAuthorize]
		public ActionResult UserAdd(RegisterViewModel model) {
			if (ModelState.IsValid) {
				SecurityData sd = new SecurityData();
				ApplicationUser user = new ApplicationUser { UserName = model.UserName, Email = model.Email };

				ExtendedUserData exUser = null;
				var result = sd.CreateApplicationUser(user, model.Password, out exUser);

				if (result == IdentityResult.Success && exUser != null) {
					result = securityHelper.UserManager.SetLockoutEnabled(exUser.Id, true);

					return RedirectToAction("UserEdit", new { @id = exUser.UserId });
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
			if (!String.IsNullOrEmpty(sXML) && sXML.Length > 500) {
				sTest = sXML.Substring(0, 250).ToLowerInvariant();

				try {
					if (sTest.Contains("<contentpageexport xmlns:xsi=\"http://www.w3.org/2001/xmlschema-instance\" xmlns:xsd=\"http://www.w3.org/2001/xmlschema\">")
						|| sTest.Contains("<contentpageexport xmlns:xsd=\"http://www.w3.org/2001/xmlschema\" xmlns:xsi=\"http://www.w3.org/2001/xmlschema-instance\">")) {
						ContentPageExport cph = ContentImportExportUtils.DeserializeContentPageExport(sXML);
						ContentImportExportUtils.AssignContentPageExportNewIDs(cph);
						ContentImportExportUtils.MapSiteCategoryTags(cph);
						ContentImportExportUtils.SaveSerializedDataExport<ContentPageExport>(cph.NewRootContentID, cph);

						if (cph.ThePage.ContentType == ContentPageType.PageType.ContentEntry) {
							//Response.Redirect(SiteFilename.PageAddEditURL + "?importid=" + cph.NewRootContentID.ToString());
							return RedirectToAction("PageAddEdit", new { importid = cph.NewRootContentID });
						} else {
							//Response.Redirect(SiteFilename.BlogPostAddEditURL + "?importid=" + cph.NewRootContentID.ToString());
							return RedirectToAction("BlogPostAddEdit", new { importid = cph.NewRootContentID });
						}
					}

					if (sTest.Contains("<siteexport xmlns:xsi=\"http://www.w3.org/2001/xmlschema-instance\" xmlns:xsd=\"http://www.w3.org/2001/xmlschema\">")
						|| sTest.Contains("<siteexport xmlns:xsd=\"http://www.w3.org/2001/xmlschema\" xmlns:xsi=\"http://www.w3.org/2001/xmlschema-instance\">")) {
						SiteExport site = ContentImportExportUtils.DeserializeSiteExport(sXML);
						ContentImportExportUtils.AssignSiteExportNewIDs(site);
						ContentImportExportUtils.SaveSerializedDataExport<SiteExport>(site.NewSiteID, site);

						//Response.Redirect(SiteFilename.SiteImportURL + "?importid=" + site.NewSiteID.ToString());
						return RedirectToAction("SiteImport", new { importid = site.NewSiteID });
					}

					if (sXML.Contains("<channel>") && sXML.Contains("<rss")) {
						int iChnl = sXML.IndexOf("<channel>");
						sTest = sXML.Substring(0, iChnl).ToLowerInvariant();
					}

					if (sTest.Contains("<!-- this is a wordpress extended rss file generated by wordpress as an export of your")
						&& sTest.Contains("http://purl.org/rss")
						&& sTest.Contains("http://wordpress.org/export")) {
						WordPressSite wps = ContentImportExportUtils.DeserializeWPExport(sXML);
						ContentImportExportUtils.AssignWPExportNewIDs(SiteData.CurrentSite, wps);
						ContentImportExportUtils.SaveSerializedDataExport<WordPressSite>(wps.NewSiteID, wps);

						//Response.Redirect(SiteFilename.SiteImportWP_URL + "?importid=" + wps.NewSiteID.ToString());
						return RedirectToAction("SiteImportWP", new { importid = wps.NewSiteID });
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

		[HttpGet]
		public ActionResult ContentSnippetHistory(Guid id) {
			ContentSnippetHistoryModel model = new ContentSnippetHistoryModel(id);

			ShowSaved();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ContentSnippetHistory(ContentSnippetHistoryModel model) {
			ModelState.Clear();

			List<Guid> lstDel = model.History.DataSource.Where(x => x.Selected).Select(x => x.ContentSnippetID).ToList();

			foreach (Guid delID in lstDel) {
				ContentSnippet.GetVersion(delID).DeleteThisVersion();
			}

			if (lstDel.Any()) {
				SetSaved();
			}

			return RedirectToAction("ContentSnippetHistory", new { @id = model.Item.Root_ContentSnippetID });
		}

		public ActionResult ChangePassword() {
			ViewBag.ChangePasswordSuccess = "";

			if (this.TempData["cmsChangePasswordSuccess"] != null) {
				ViewBag.ChangePasswordSuccess = this.TempData["cmsChangePasswordSuccess"].ToString();
			}

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model) {
			ViewBag.ChangePasswordSuccess = "";

			if (!ModelState.IsValid) {
				return View(model);
			}

			var result = await securityHelper.UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

			if (result.Succeeded) {
				var user = await securityHelper.UserManager.FindByIdAsync(User.Identity.GetUserId());
				if (user != null) {
					await securityHelper.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
				}

				this.TempData["cmsChangePasswordSuccess"] = "Password change success!";

				return RedirectToAction("ChangePassword", new { Message = ManageMessageId.ChangePasswordSuccess });
			}

			AddErrors(result);
			return View(model);
		}

		[AllowAnonymous]
		public ActionResult DatabaseSetup(string signout) {
			DatabaseSetupModel model = new DatabaseSetupModel();

			DatabaseUpdate du = new DatabaseUpdate(true);

			if (!String.IsNullOrEmpty(signout)) {
				SignOut();
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

				try {
					model.CreateUser = !DatabaseUpdate.UsersExist;
				} catch { }

				if (bUpdate) {
					DatabaseUpdateStatus status = du.PerformUpdates();
					lst = du.MergeMessages(lst, status.Messages);
				} else {
					DataInfo ver = DatabaseUpdate.GetDbSchemaVersion();
					du.HandleResponse(lst, "Database up-to-date [" + ver.DataValue + "] ");
				}

				bUpdate = du.DatabaseNeedsUpdate();

				if (!bUpdate && DatabaseUpdate.LastSQLError == null) {
					model.CreateUser = !DatabaseUpdate.UsersExist;
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
		public ActionResult CreateFirstAdmin() {
			RedirectIfUsersExist();

			if (SecurityData.IsAuthenticated) {
				SignOut();

				return RedirectToAction("CreateFirstAdmin", new { @signout = true });
			}

			RegisterViewModel model = new RegisterViewModel();

			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult CreateFirstAdmin(RegisterViewModel model) {
			RedirectIfUsersExist();

			if (ModelState.IsValid) {
				SignOut();

				SecurityData sd = new SecurityData();
				ApplicationUser user = new ApplicationUser { UserName = model.UserName, Email = model.Email };

				ExtendedUserData exUser = null;
				var result = sd.CreateApplicationUser(user, model.Password, out exUser);

				if (result.Succeeded) {
					SecurityData.AddUserToRole(model.UserName, SecurityData.CMSGroup_Admins);
					SecurityData.AddUserToRole(model.UserName, SecurityData.CMSGroup_Users);

					return RedirectToAction("Index");
				}

				AddErrors(result);
			}

			Helper.HandleErrorDict(ModelState);
			return View(model);
		}

		public ActionResult CheckDatabase() {
			if (DatabaseUpdate.AreCMSTablesIncomplete() || !DatabaseUpdate.UsersExist) {
				DatabaseUpdate.ResetFailedSQL();
				DatabaseUpdate.ResetSQLState();

				// Response.Redirect(SiteFilename.DatabaseSetupURL);
				return RedirectToAction("DatabaseSetup");
			}

			return null;
		}

		[AllowAnonymous]
		public ActionResult Login(string returnUrl) {
			var res = CheckDatabase();
			if (res != null) {
				return res;
			}

			LoginViewModel model = new LoginViewModel();
			model.ReturnUrl = HttpUtility.UrlEncode(returnUrl);

			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginViewModel model) {
			Helper.ForceValidation(ModelState, model);

			if (!ModelState.IsValid) {
				Helper.HandleErrorDict(ModelState);

				return View(model);
			}

			string returnUrl = HttpUtility.UrlDecode(model.ReturnUrl);

			//TODO: make configurable
			//manage.UserManager.UserLockoutEnabledByDefault = true;
			//manage.UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;
			//manage.UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(15);

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, change to shouldLockout: true
			var user = await securityHelper.UserManager.FindByNameAsync(model.UserName);

			var result = await securityHelper.SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: true);

			switch (result) {
				case SignInStatus.Success:
					await securityHelper.UserManager.ResetAccessFailedCountAsync(user.Id);

					return RedirectToLocal(returnUrl);

				case SignInStatus.LockedOut:
					return View("Lockout");

				case SignInStatus.RequiresVerification:
					return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

				case SignInStatus.Failure:
				default:
					ModelState.AddModelError(String.Empty, "Invalid login attempt.");

					if (user != null && user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value < DateTime.UtcNow) {
						user.LockoutEndDateUtc = null;
						user.AccessFailedCount = 1;
						securityHelper.UserManager.Update(user);
					}

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
				Helper.HandleErrorDict(ModelState);

				return View(model);
			}

			//var user = await UserManager.FindByNameAsync(model.Email);
			var user = await securityHelper.UserManager.FindByEmailAsync(model.Email);
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

			Helper.HandleErrorDict(ModelState);
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
			if (ModelState.IsValid) {
				var user = await securityHelper.UserManager.FindByEmailAsync(model.Email);
				if (user == null) {
					// Don't reveal that the user does not exist or is not confirmed
					return View("ForgotPasswordConfirmation");
				} else {
					SecurityData sd = new SecurityData();
					sd.ResetPassword(model.Email);
					return RedirectToAction("ForgotPasswordConfirmation");
				}
			}

			Helper.HandleErrorDict(ModelState);
			// If we got this far, something failed, redisplay form
			return View(model);
		}

		public ActionResult Index() {
			var res = CheckDatabase();
			if (res != null) {
				return res;
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

		[AllowAnonymous]
		public ActionResult About() {
			return View();
		}

		protected void LoadTimeZoneInfo() {
			var now = DateTime.Now;

			if (SiteData.CurrentSite != null) {
				// site is pre-existing
				now = SiteData.CurrentSite.Now;
			}

			var lstTZ = TimeZoneInfo.GetSystemTimeZones();

			ViewBag.TimeZoneInfoList = (from z in lstTZ
										select new {
											Id = z.Id,
											DisplayNameCurrent2 = "(UTC" +
																	(z.GetUtcOffset(now).Hours != 0 ?
																			(z.GetUtcOffset(now).Hours >= 0 ? "+" : "-") + z.GetUtcOffset(now).ToString("hh\\:mm")
																			: String.Empty) + ") "
																	+ (z.IsDaylightSavingTime(now) ? z.DaylightName : z.StandardName),
											DisplayNameCurrent = z.DisplayName,
											DisplayName = z.DisplayName,
											UtcOffset = z.GetUtcOffset(now),
											StandardName = z.StandardName,
											DaylightName = z.DaylightName
										}).OrderBy(x => x.DisplayNameCurrent).OrderBy(x => x.UtcOffset).ToList();
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
		[CmsAdminAuthorize]
		public ActionResult SiteDetail(Guid id) {
			SiteModel model = new SiteModel(id);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[CmsAdminAuthorize]
		public ActionResult SiteDetail(SiteModel model) {
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[CmsAdminAuthorize]
		public ActionResult SiteAddUser(SiteModel model) {
			ModelState.Clear();

			if (String.IsNullOrEmpty(model.NewUserId)) {
				ModelState.AddModelError("NewUserId", "The New User field is required.");
			}

			SiteData site = model.Site;
			Helper.ForceValidation(ModelState, model);

			if (ModelState.IsValid) {
				if (!String.IsNullOrEmpty(model.NewUserId)) {
					ExtendedUserData exUsr = new ExtendedUserData(new Guid(model.NewUserId));
					exUsr.AddToSite(site.SiteID);

					if (model.NewUserAsEditor) {
						exUsr.AddToRole(SecurityData.CMSGroup_Editors);
					}
				}

				return RedirectToAction("SiteDetail", new { @id = site.SiteID });
			}

			Helper.HandleErrorDict(ModelState);

			model.LoadUsers();

			return View("SiteDetail", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[CmsAdminAuthorize]
		public ActionResult SiteRemoveUsers(SiteModel model) {
			ModelState.Clear();

			SiteData site = model.Site;

			if (ModelState.IsValid) {
				List<UserModel> usrs = model.Users.Where(x => x.Selected).ToList();

				foreach (var u in usrs) {
					ExtendedUserData exUsr = new ExtendedUserData(u.User.UserId);
					exUsr.RemoveFromSite(site.SiteID);
				}

				return RedirectToAction("SiteDetail", new { @id = site.SiteID });
			}

			Helper.HandleErrorDict(ModelState);

			return View("SiteDetail", model);
		}

		[HttpGet]
		public ActionResult SiteInfo() {
			var res = CheckDatabase();
			if (res != null) {
				return res;
			}

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

				var timezoneOld = site.TimeZoneIdentifier;
				var datePatternOld = site.Blog_DatePattern;

				site = model.Site;

				site.Save();

				if (datePatternOld != model.Site.Blog_DatePattern || timezoneOld != model.Site.TimeZoneIdentifier) {
					using (ContentPageHelper cph = new ContentPageHelper()) {
						cph.BulkBlogFileNameUpdateFromDate(this.SiteID);
					}
				}

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
		public ActionResult PageEdit(Guid id) {
			ContentPageModel model = new ContentPageModel();

			cmsHelper.OverrideKey(id);
			ContentPage pageContents = cmsHelper.cmsAdminContent;

			model.SetPage(pageContents);

			ShowSaved();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PageEdit(ContentPageModel model) {
			cmsHelper.OverrideKey(model.ContentPage.Root_ContentID);

			ContentPage page = model.ContentPage;

			var pageContents = cmsHelper.cmsAdminContent;

			pageContents.GoLiveDate = page.GoLiveDate;
			pageContents.RetireDate = page.RetireDate;

			pageContents.IsLatestVersion = true;
			pageContents.Thumbnail = page.Thumbnail;

			pageContents.TitleBar = page.TitleBar;
			pageContents.NavMenuText = page.NavMenuText;
			pageContents.PageHead = page.PageHead;
			pageContents.PageSlug = null;

			pageContents.MetaDescription = page.MetaDescription;
			pageContents.MetaKeyword = page.MetaKeyword;

			pageContents.EditDate = SiteData.CurrentSite.Now;
			pageContents.NavOrder = page.NavOrder;

			pageContents.PageActive = page.PageActive;
			pageContents.ShowInSiteNav = page.ShowInSiteNav;
			pageContents.ShowInSiteMap = page.ShowInSiteMap;
			pageContents.BlockIndex = page.BlockIndex;

			pageContents.CreditUserId = page.CreditUserId;

			pageContents.EditUserId = SecurityData.CurrentUserGuid;

			model.SetPage(pageContents);

			Helper.ForceValidation(ModelState, model);

			if (ModelState.IsValid) {
				cmsHelper.cmsAdminContent = pageContents;
				SetSaved();
				return RedirectToAction("PageEdit", new { @id = model.ContentPage.Root_ContentID });
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpGet]
		public ActionResult PageAddEdit(Guid? id, Guid? versionid, Guid? importid, string mode) {
			ContentPageModel model = new ContentPageModel();
			ContentPage pageContents = model.GetPage(id, versionid, importid, mode);
			ViewBag.ContentEditMode = model.Mode;

			if (pageContents.ContentType != ContentPageType.PageType.ContentEntry) {
				return RedirectToAction("BlogPostAddEdit", new { @id = pageContents.Root_ContentID, @mode = model.Mode });
			}

			return View(model);
		}

		[HttpGet]
		public ActionResult BlogPostEdit(Guid id) {
			ContentPageModel model = new ContentPageModel();

			cmsHelper.OverrideKey(id);
			ContentPage pageContents = cmsHelper.cmsAdminContent;

			model.SetPage(pageContents);

			ShowSaved();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult BlogPostEdit(ContentPageModel model) {
			cmsHelper.OverrideKey(model.ContentPage.Root_ContentID);

			ContentPage page = model.ContentPage;

			var pageContents = cmsHelper.cmsAdminContent;

			pageContents.GoLiveDate = page.GoLiveDate;
			pageContents.RetireDate = page.RetireDate;

			pageContents.IsLatestVersion = true;
			pageContents.Thumbnail = page.Thumbnail;

			pageContents.TitleBar = page.TitleBar;
			pageContents.NavMenuText = page.NavMenuText;
			pageContents.PageHead = page.PageHead;

			pageContents.MetaDescription = page.MetaDescription;
			pageContents.MetaKeyword = page.MetaKeyword;

			pageContents.EditDate = SiteData.CurrentSite.Now;
			pageContents.NavOrder = page.NavOrder;

			pageContents.PageActive = page.PageActive;
			pageContents.ShowInSiteNav = page.ShowInSiteNav;
			pageContents.ShowInSiteMap = page.ShowInSiteMap;
			pageContents.BlockIndex = page.BlockIndex;

			pageContents.CreditUserId = page.CreditUserId;

			pageContents.EditUserId = SecurityData.CurrentUserGuid;
			pageContents.Parent_ContentID = null;

			pageContents.ContentCategories = (from l in SiteData.CurrentSite.GetCategoryList()
											  join cr in model.SelectedCategories on l.ContentCategoryID.ToString().ToLowerInvariant() equals cr.ToLowerInvariant()
											  select l).ToList();
			pageContents.ContentTags = (from l in SiteData.CurrentSite.GetTagList()
										join cr in model.SelectedTags on l.ContentTagID.ToString().ToLowerInvariant() equals cr.ToLowerInvariant()
										select l).ToList();

			model.SetPage(pageContents);

			Helper.ForceValidation(ModelState, model);

			if (ModelState.IsValid) {
				cmsHelper.cmsAdminContent = pageContents;
				SetSaved();
				return RedirectToAction("BlogPostEdit", new { @id = model.ContentPage.Root_ContentID });
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult PageAddEdit(ContentPageModel model) {
			Helper.ForceValidation(ModelState, model);
			model.Mode = (String.IsNullOrEmpty(model.Mode) || model.Mode.Trim().ToLowerInvariant() != "raw") ? "html" : "raw";
			ViewBag.ContentEditMode = model.Mode;

			if (ModelState.IsValid) {
				var pageContents = model.SavePage();

				if (model.VisitPage) {
					Response.Redirect(pageContents.FileName);
				} else {
					return RedirectToAction("PageAddEdit", new { @id = pageContents.Root_ContentID, @mode = model.Mode });
				}
			}

			Helper.HandleErrorDict(ModelState);
			model.RefreshWidgetList();

			return View(model);
		}

		[HttpGet]
		public ActionResult BlogPostAddEdit(Guid? id, Guid? versionid, Guid? importid, string mode) {
			ContentPageModel model = new ContentPageModel();
			ContentPage pageContents = model.GetPost(id, versionid, importid, mode);
			ViewBag.ContentEditMode = model.Mode;

			if (pageContents.ContentType != ContentPageType.PageType.BlogEntry) {
				return RedirectToAction("PageAddEdit", new { @id = pageContents.Root_ContentID, @mode = model.Mode });
			}

			return View(model);
		}

		[HttpGet]
		public ActionResult ContentSnippetAddEdit(Guid? id, Guid? versionid, string mode) {
			ViewBag.ContentEditMode = (String.IsNullOrEmpty(mode) || mode.Trim().ToLowerInvariant() != "raw") ? "html" : "raw";

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
			ViewBag.ContentEditMode = (String.IsNullOrEmpty(mode) || mode.Trim().ToLowerInvariant() != "raw") ? "html" : "raw";
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
		public ActionResult DeleteContentSnippet(ContentSnippet model) {
			ContentSnippet item = ContentSnippet.Get(model.Root_ContentSnippetID);
			item.Delete();

			return RedirectToAction("ContentSnippetIndex");
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult BlogPostAddEdit(ContentPageModel model) {
			Helper.ForceValidation(ModelState, model);
			model.Mode = (String.IsNullOrEmpty(model.Mode) || model.Mode.Trim().ToLowerInvariant() != "raw") ? "html" : "raw";
			ViewBag.ContentEditMode = model.Mode;

			if (ModelState.IsValid) {
				var pageContents = model.SavePost();

				if (model.VisitPage) {
					Response.Redirect(pageContents.FileName);
				} else {
					return RedirectToAction("BlogPostAddEdit", new { @id = pageContents.Root_ContentID, @mode = model.Mode });
				}
			}

			Helper.HandleErrorDict(ModelState);
			model.RefreshWidgetList();

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
		public ActionResult ControlPropertiesEdit(Guid id, Guid pageid) {
			cmsHelper.OverrideKey(pageid);

			Widget w = (from aw in cmsHelper.cmsAdminWidget
						where aw.Root_WidgetID == id
						orderby aw.WidgetOrder, aw.EditDate
						select aw).FirstOrDefault();

			List<ObjectProperty> lstProps = ObjectProperty.GetWidgetProperties(w, pageid);

			WidgetProperties model = new WidgetProperties(w, lstProps);

			ShowSaved();

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
						int checkedPosition = 0;
						foreach (var v in itm.Options) {
							if (v.Selected) {
								var pp = new WidgetProps();
								pp.KeyName = String.Format("{0}|{1}", itm.Name, checkedPosition);
								pp.KeyValue = v.Key.ToString();
								props.Add(pp);
								checkedPosition++;
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

			SetSaved();

			return RedirectToAction("ControlPropertiesEdit", new { @id = w.Root_WidgetID, @pageid = w.Root_ContentID });
		}

		protected bool? GetSaved() {
			bool? saved = TempData["cmsShowSaved"] != null ? (bool?)TempData["cmsShowSaved"] : null;

			return saved;
		}

		protected void SetSaved() {
			SetSaved(true);
		}

		protected void SetSaved(bool? v) {
			TempData["cmsShowSaved"] = v;
		}

		protected void ShowSaved() {
			bool? saved = GetSaved();

			if (saved.HasValue && saved.Value) {
				ViewBag.SavedPageAlert = true;
				ViewBag.SavedPageAlertText = "Changes Applied";
			}
		}

		protected void ShowSaved(string msg) {
			bool? saved = GetSaved();

			if (saved.HasValue && saved.Value) {
				ViewBag.SavedPageAlert = true;
				ViewBag.SavedPageAlertText = msg;
			}
		}

		protected void ShowSaved(string msgTrue, string msgFalse) {
			bool? saved = GetSaved();

			if (saved.HasValue) {
				ViewBag.SavedPageAlert = true;
				if (saved.Value) {
					ViewBag.SavedPageAlertText = msgTrue;
				} else {
					ViewBag.SavedPageAlertText = msgFalse;
				}
			}
		}

		[HttpGet]
		public ActionResult SiteMapPop() {
			ShowSaved();

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

			SetSaved(saved);

			return RedirectToAction(viewName);
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

			var pageContentsParent = pageHelper.FindContentByID(this.SiteID, id);
			var pageContents = new ContentPage(this.SiteID, ContentPageType.PageType.ContentEntry);

			if (pageContentsParent != null && pageContentsParent.ContentType == ContentPageType.PageType.ContentEntry) {
				pageContents.Parent_ContentID = id;
			} else {
				pageContents.Parent_ContentID = Guid.Empty;
			}

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

				ShowSaved("Page Created");
				model.VisitPage = true;
			}

			Helper.HandleErrorDict(ModelState);
			return View(model);
		}

		[HttpGet]
		public ActionResult PageChildSort(Guid id) {
			ShowSaved();

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

				SetSaved();
				return RedirectToAction("PageChildSort", new { @id = model.Root_ContentID });
			}
		}

		[HttpGet]
		public ActionResult ContentEdit(Guid id, Guid? widgetid, string field, string mode) {
			ContentSingleModel model = new ContentSingleModel();
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

			ShowSaved();

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

				SetSaved();

				return RedirectToAction("ContentEdit", new { @id = model.PageId, @widgetid = model.WidgetId, @field = model.Field, @mode = model.Mode });
			}

			return View(model);
		}

		[HttpGet]
		public ActionResult PageHistory(Guid? id, Guid? versionid) {
			ShowSaved("Selected items removed", "No items selected to remove");

			PageHistoryModel model = new PageHistoryModel(this.SiteID);

			if (id.HasValue) {
				model.SetCurrent(id.Value);
			}
			if (versionid.HasValue) {
				model.SetVersion(versionid.Value);
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PageHistory(PageHistoryModel model) {
			ModelState.Clear();
			List<Guid> lstDel = model.History.DataSource.Where(x => x.Selected).Select(x => x.ContentID).ToList();

			if (lstDel.Any()) {
				pageHelper.RemoveVersions(this.SiteID, lstDel);
				SetSaved(true);
			} else {
				SetSaved(false);
			}

			return RedirectToAction("PageHistory", new { @id = model.Root_ContentID });
		}

		[HttpGet]
		public ActionResult SiteContentStatusChange() {
			SiteContentStatusChangeModel model = new SiteContentStatusChangeModel();

			return SiteContentStatusChange(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteContentStatusChange(SiteContentStatusChangeModel model) {
			List<ContentPage> lstContent = null;
			int dateRangeDays = model.SelectedRange;

			if (!model.UseDate) {
				dateRangeDays = -1;
			}

			if (model.PerformSave && model.Pages != null && model.Pages.Any()) {
				List<Guid> lstUpd = model.Pages.Where(x => x.Selected).Select(x => x.Root_ContentID).ToList();

				if (lstUpd.Any()) {
					string sAct = model.SelectedAction.ToLowerInvariant();

					if (sAct != "none") {
						ContentPageHelper.UpdateField fieldDev = ContentPageHelper.UpdateField.MarkActive;

						if (sAct == "inactive") {
							fieldDev = ContentPageHelper.UpdateField.MarkInactive;
						}

						if (sAct == "searchengine") {
							fieldDev = ContentPageHelper.UpdateField.MarkAsIndexable;
						}
						if (sAct == "sitemap") {
							fieldDev = ContentPageHelper.UpdateField.MarkIncludeInSiteMap;
						}
						if (sAct == "navigation") {
							fieldDev = ContentPageHelper.UpdateField.MarkIncludeInSiteNav;
						}

						if (sAct == "searchengine-no") {
							fieldDev = ContentPageHelper.UpdateField.MarkAsIndexableNo;
						}
						if (sAct == "sitemap-no") {
							fieldDev = ContentPageHelper.UpdateField.MarkIncludeInSiteMapNo;
						}
						if (sAct == "navigation-no") {
							fieldDev = ContentPageHelper.UpdateField.MarkIncludeInSiteNavNo;
						}

						pageHelper.MarkSelectedPublished(SiteData.CurrentSiteID, lstUpd, fieldDev);
					}

					ModelState.Clear();
					//return RedirectToAction("SiteContentStatusChange");
				}
			}

			model.SelectedAction = String.Empty;

			lstContent = pageHelper.GetContentByDateRange(this.SiteID, model.SearchDate, dateRangeDays, model.PageType,
							model.PageActive, model.ShowInSiteMap, model.ShowInSiteNav, model.BlockIndex);

			model.Pages = lstContent;

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
				ExportWhat = (SiteExport.ExportType)Enum.Parse(typeof(SiteExport.ExportType), exportwhat, true);
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
		public ActionResult PageTemplateUpdate() {
			CMSConfigHelper.CleanUpSerialData();
			PageTemplateUpdateModel model = new PageTemplateUpdateModel();
			model.SelectedSearch = PageTemplateUpdateModel.SearchBy.Filtered;

			PagedData<ContentPage> pagedData = new PagedData<ContentPage>();
			pagedData.PageSize = 10;
			pagedData.InitOrderBy(x => x.NavMenuText);
			model.Page = pagedData;

			return PageTemplateUpdate(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PageTemplateUpdate(PageTemplateUpdateModel model) {
			PagedData<ContentPage> pagedData = model.Page;

			if (!String.IsNullOrEmpty(model.SelectedTemplate)) {
				List<Guid> lstUpd = pagedData.DataSource.Where(x => x.Selected).Select(x => x.Root_ContentID).ToList();

				if (lstUpd.Any()) {
					pageHelper.BulkUpdateTemplate(this.SiteID, lstUpd, model.SelectedTemplate);

					//return RedirectToAction("PageTemplateUpdate");
				}

				model.SelectedTemplate = String.Empty;
			}

			pagedData.InitOrderBy(x => x.NavMenuText);
			pagedData.ToggleSort();
			var srt = pagedData.ParseSort();

			IQueryable<ContentPage> query = null;

			if (model.SelectedSearch == PageTemplateUpdateModel.SearchBy.AllPages) {
				query = pageHelper.GetAllLatestContentList(this.SiteID).AsQueryable();
			} else {
				if (!model.ParentPageID.HasValue) {
					query = pageHelper.GetTopNavigation(this.SiteID, false).AsQueryable();
				} else {
					query = pageHelper.GetParentWithChildNavigation(this.SiteID, model.ParentPageID.Value, false).AsQueryable();
				}
			}

			query = query.SortByParm<ContentPage>(srt.SortField, srt.SortDirection);
			pagedData.DataSource = query.ToList();

			pagedData.TotalRecords = pagedData.DataSource.Count();
			pagedData.PageSize = 1 + (pagedData.TotalRecords * 2);

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
		public ActionResult WidgetTime(Guid? id, Guid widgetid) {
			WidgetEditModel model = new WidgetEditModel(id, widgetid);

			ShowSaved();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult WidgetTime(WidgetEditModel model) {
			if (ModelState.IsValid) {
				model.Save();

				SetSaved();

				if (model.CachedWidget) {
					return RedirectToAction("WidgetTime", new { @id = model.Root_ContentID, @widgetid = model.Root_WidgetID });
				}

				return RedirectToAction("WidgetTime", new { @widgetid = model.Root_WidgetID });
			}

			return View(model);
		}

		[HttpGet]
		public ActionResult WidgetHistory(Guid id, bool? saved) {
			ShowSaved("Selected items removed", "No items selected to remove");

			WidgetHistoryModel model = new WidgetHistoryModel(id);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult WidgetHistory(WidgetHistoryModel model) {
			ModelState.Clear();

			SetSaved(model.Remove());

			return RedirectToAction("WidgetHistory", new { @id = model.Root_WidgetID });
		}

		[HttpGet]
		public ActionResult DuplicateWidgetFrom(Guid id, string zone) {
			DuplicateWidgetFromModel model = new DuplicateWidgetFromModel(id, zone);

			ShowSaved();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DuplicateWidgetFrom(DuplicateWidgetFromModel model) {
			if (ModelState.IsValid) {
				if (model.StepNumber == 1) {
					model.SearchOne();
				}
				if (model.StepNumber == 2) {
					model.SearchTwo();
				}
				if (model.StepNumber == 3) {
					model.Save();
				}

				ModelState.Clear();
			}

			Helper.HandleErrorDict(ModelState);
			return View(model);
		}

		[HttpGet]
		public ActionResult WidgetList(Guid id, string zone) {
			WidgetListModel model = new WidgetListModel();
			model.Root_ContentID = id;
			model.PlaceholderName = zone;
			cmsHelper.OverrideKey(model.Root_ContentID);

			ShowSaved();

			model.Controls = (from aw in cmsHelper.cmsAdminWidget
							  where aw.PlaceholderName.ToLowerInvariant() == model.PlaceholderName.ToLowerInvariant()
									|| model.PlaceholderName.ToLowerInvariant() == "cms-all-placeholder-zones"
							  orderby aw.PlaceholderName ascending, aw.IsWidgetPendingDelete ascending, aw.IsWidgetActive descending, aw.WidgetOrder
							  select aw).ToList();

			ModelState.Clear();

			return View(model);
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
			}

			SetSaved();

			return RedirectToAction("WidgetList", new { @id = model.Root_ContentID, @zone = model.PlaceholderName });
		}

		[HttpGet]
		public ActionResult PageWidgets(Guid id) {
			WidgetListModel model = new WidgetListModel(id);
			model.PlaceholderName = String.Empty;

			ShowSaved();

			return PageWidgets(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PageWidgets(WidgetListModel model) {
			List<Guid> lsActive = model.Controls.Where(x => x.IsWidgetActive).Select(x => x.Root_WidgetID).ToList();
			widgetHelper.SetStatusList(model.Root_ContentID, lsActive, true);

			List<Guid> lsInactive = model.Controls.Where(x => !x.IsWidgetActive).Select(x => x.Root_WidgetID).ToList();
			widgetHelper.SetStatusList(model.Root_ContentID, lsInactive, false);

			model = new WidgetListModel(model.Root_ContentID);

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
		public ActionResult SiteSkinIndex() {
			PagedData<CMSTemplate> model = new PagedData<CMSTemplate>();
			model.PageSize = 1000;
			model.InitOrderBy(x => x.Caption);

			return SiteSkinIndex(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteSkinIndex(PagedData<CMSTemplate> model) {
			model.ToggleSort();
			var templates = cmsHelper.Templates.Where(x => x.TemplatePath.ToLowerInvariant() != SiteData.DefaultTemplateFilename.ToLowerInvariant()).ToList();
			var srt = model.ParseSort();

			var query = ReflectionUtilities.SortByParm<CMSTemplate>(templates, srt.SortField, srt.SortDirection);

			model.TotalRecords = -1;
			model.DataSource = query.ToList();

			ModelState.Clear();

			return View(model);
		}

		[HttpGet]
		public ActionResult SiteSkinEdit(string path, string alt) {
			SiteSkinModel model = null;

			if (String.IsNullOrEmpty(alt)) {
				model = new SiteSkinModel(path);
			} else {
				model = new SiteSkinModel(path, alt);
			}

			model.ReadFile();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult SiteSkinEdit(SiteSkinModel model) {
			if (ModelState.IsValid) {
				model.SaveFile();

				if (!String.IsNullOrEmpty(model.AltPath)) {
					return RedirectToAction("SiteSkinEdit", new { @path = model.EncodedPath, @alt = model.EncodePath(model.AltPath) });
				}
				return RedirectToAction("SiteSkinEdit", new { @path = model.EncodedPath });
			}

			Helper.HandleErrorDict(ModelState);

			model.ReadRelated();

			return View(model);
		}

		[HttpGet]
		public ActionResult PageCommentIndex(Guid? id) {
			CommentIndexModel model = new CommentIndexModel();
			model.Comments.PageSize = 25;
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
		public ActionResult BlogPostCommentIndex(Guid? id) {
			CommentIndexModel model = new CommentIndexModel();
			model.Comments.PageSize = 25;
			model.PageType = ContentPageType.PageType.BlogEntry;
			model.Root_ContentID = id;

			return CommentIndex(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult BlogPostCommentIndex(CommentIndexModel model) {
			model.PageType = ContentPageType.PageType.BlogEntry;

			return CommentIndex(model);
		}

		[HttpGet]
		public ActionResult CommentAddEdit(Guid id, bool? pageComment) {
			PostComment model1 = PostComment.GetContentCommentByID(id);
			PostCommentModel model = null;

			if (pageComment.HasValue && pageComment.Value) {
				model = new PostCommentModel(model1, PostCommentModel.ViewType.PageView);
			} else {
				if (model1.ContentType == ContentPageType.PageType.BlogEntry) {
					model = new PostCommentModel(model1, PostCommentModel.ViewType.BlogIndex);
				} else {
					model = new PostCommentModel(model1, PostCommentModel.ViewType.ContentIndex);
				}
			}

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public ActionResult CommentAddEdit(PostCommentModel model) {
			PostComment comment = model.Comment;

			if (ModelState.IsValid) {
				PostComment model2 = PostComment.GetContentCommentByID(comment.ContentCommentID);
				model2.CommenterEmail = comment.CommenterEmail;
				model2.CommenterName = comment.CommenterName;
				model2.CommenterURL = comment.CommenterURL ?? String.Empty;

				model2.IsApproved = comment.IsApproved;
				model2.IsSpam = comment.IsSpam;
				model2.PostCommentText = comment.PostCommentText ?? String.Empty;

				model2.Save();

				if (model.ViewMode == PostCommentModel.ViewType.PageView) {
					return RedirectToAction("CommentAddEdit", new { @id = comment.ContentCommentID, @pageComment = true });
				}

				return RedirectToAction("CommentAddEdit", new { @id = comment.ContentCommentID });
			}

			Helper.HandleErrorDict(ModelState);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteCommentAddEdit(PostCommentModel model) {
			ModelState.Clear();

			var model2 = PostComment.GetContentCommentByID(model.Comment.ContentCommentID);

			model2.Delete();

			if (model.ViewMode == PostCommentModel.ViewType.PageView) {
				if (model.Comment.ContentType == ContentPageType.PageType.BlogEntry) {
					return RedirectToAction("BlogPostCommentIndex", new { @id = model.Root_ContentID });
				} else {
					return RedirectToAction("PageCommentIndex", new { @id = model.Root_ContentID });
				}
			}

			if (model.Comment.ContentType == ContentPageType.PageType.BlogEntry) {
				return RedirectToAction("BlogPostCommentIndex");
			} else {
				return RedirectToAction("PageCommentIndex");
			}
		}

		protected ActionResult CommentIndex(CommentIndexModel model) {
			PagedData<PostComment> pagedData = model.Comments;

			pagedData.ToggleSort();

			if (!model.Root_ContentID.HasValue) {
				pagedData.TotalRecords = PostComment.GetCommentCountBySiteAndType(SiteData.CurrentSiteID, model.PageType, model.IsApproved, model.IsSpam);
			} else {
				pagedData.TotalRecords = PostComment.GetCommentCountByContent(model.Root_ContentID.Value, model.IsApproved, model.IsSpam);
			}

			if (!model.Root_ContentID.HasValue) {
				pagedData.DataSource = PostComment.GetCommentsBySitePageNumber(SiteData.CurrentSiteID, pagedData.PageNumberZeroIndex, pagedData.PageSize, pagedData.OrderBy, model.PageType, model.IsApproved, model.IsSpam);
			} else {
				pagedData.DataSource = PostComment.GetCommentsByContentPageNumber(model.Root_ContentID.Value, pagedData.PageNumberZeroIndex, pagedData.PageSize, pagedData.OrderBy, model.IsApproved, model.IsSpam);
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
		public ActionResult BlogPostTemplateUpdate() {
			CMSConfigHelper.CleanUpSerialData();
			PostTemplateUpdateModel model = new PostTemplateUpdateModel();
			model.SelectedSearch = PostTemplateUpdateModel.SearchBy.Filtered;

			PagedData<ContentPage> pagedData = new PagedData<ContentPage>();
			pagedData.PageSize = 10;
			pagedData.InitOrderBy(x => x.GoLiveDate, false);
			model.Page = pagedData;

			return BlogPostTemplateUpdate(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult BlogPostTemplateUpdate(PostTemplateUpdateModel model) {
			PagedData<ContentPage> pagedData = model.Page;

			if (!String.IsNullOrEmpty(model.SelectedTemplate)) {
				List<Guid> lstUpd = pagedData.DataSource.Where(x => x.Selected).Select(x => x.Root_ContentID).ToList();

				if (lstUpd.Any()) {
					pageHelper.BulkUpdateTemplate(this.SiteID, lstUpd, model.SelectedTemplate);

					//return RedirectToAction("BlogPostTemplateUpdate");
				}

				model.SelectedTemplate = String.Empty;
			}

			pagedData.InitOrderBy(x => x.GoLiveDate, false);
			pagedData.ToggleSort();
			var srt = pagedData.ParseSort();

			IQueryable<ContentPage> query = null;

			if (model.SelectedSearch == PostTemplateUpdateModel.SearchBy.AllPages) {
				query = pageHelper.GetAllLatestBlogList(this.SiteID).AsQueryable();
			} else {
				query = pageHelper.GetPostsByDateRange(this.SiteID, model.SearchDate, model.SelectedRange, false).AsQueryable();
			}

			query = query.SortByParm<ContentPage>(srt.SortField, srt.SortDirection);
			pagedData.DataSource = query.ToList();

			pagedData.TotalRecords = pagedData.DataSource.Count();
			pagedData.PageSize = 1 + (pagedData.TotalRecords * 2);

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
		[CmsAdminAuthorize]
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
		[CmsAdminAuthorize]
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
		[CmsAdminAuthorize]
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
		[CmsAdminAuthorize]
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

		[HttpGet]
		public ActionResult SiteTemplateUpdate() {
			SiteTemplateUpdateModel model = new SiteTemplateUpdateModel();

			ContentPage pageHome = pageHelper.FindHome(this.SiteID, true);
			ContentPage pageIndex = null;

			if (pageHome != null) {
				model.HomePage = pageHome.TemplateFile.ToLowerInvariant();
				model.HomePageTitle = pageHome.NavMenuText;
				model.HomePageLink = pageHome.FileName;

				if (SiteData.CurrentSite.Blog_Root_ContentID.HasValue) {
					pageIndex = pageHelper.FindContentByID(this.SiteID, SiteData.CurrentSite.Blog_Root_ContentID.Value);
					model.IndexPageID = SiteData.CurrentSite.Blog_Root_ContentID.Value;
					model.IndexPage = pageIndex.TemplateFile.ToLowerInvariant();
				}
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult SiteTemplateUpdate(SiteTemplateUpdateModel model) {
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

			this.ViewData[PagePayload.ViewDataKey] = _page;

			return View(SiteData.PreviewTemplateFile);
		}

		protected void AddErrors(IdentityResult result) {
			Helper.AddErrors(ModelState, result);
		}

		private void RedirectIfUsersExist() {
			if (DatabaseUpdate.UsersExist) {
				Response.Redirect(SiteFilename.DashboardURL);
				//return RedirectToAction("Dashboard");
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

		protected void SignOut() {
			securityHelper.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (securityHelper != null) {
				securityHelper.Dispose();
			}
			if (pageHelper != null) {
				pageHelper.Dispose();
			}
			if (widgetHelper != null) {
				widgetHelper.Dispose();
			}
			if (cmsHelper != null) {
				cmsHelper.Dispose();
			}
		}
	}
}