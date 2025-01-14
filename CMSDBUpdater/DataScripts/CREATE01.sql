GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_CategoryContentMapping](
	[CategoryContentMappingID] [uniqueidentifier] NOT NULL,
	[ContentCategoryID] [uniqueidentifier] NOT NULL,
	[Root_ContentID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_carrot_CategoryContentMapping] PRIMARY KEY NONCLUSTERED 
(
	[CategoryContentMappingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_Content](
	[ContentID] [uniqueidentifier] NOT NULL,
	[Root_ContentID] [uniqueidentifier] NOT NULL,
	[Parent_ContentID] [uniqueidentifier] NULL,
	[IsLatestVersion] [bit] NOT NULL,
	[TitleBar] [nvarchar](256) NULL,
	[NavMenuText] [nvarchar](256) NULL,
	[PageHead] [nvarchar](256) NULL,
	[PageText] [nvarchar](max) NULL,
	[LeftPageText] [nvarchar](max) NULL,
	[RightPageText] [nvarchar](max) NULL,
	[NavOrder] [int] NOT NULL,
	[EditUserId] [uniqueidentifier] NULL,
	[EditDate] [datetime] NOT NULL,
	[TemplateFile] [nvarchar](256) NULL,
	[MetaKeyword] [nvarchar](1024) NULL,
	[MetaDescription] [nvarchar](1024) NULL,
	[CreditUserId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_carrot_Content] PRIMARY KEY CLUSTERED 
(
	[ContentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_ContentCategory](
	[ContentCategoryID] [uniqueidentifier] NOT NULL,
	[SiteID] [uniqueidentifier] NOT NULL,
	[CategoryText] [nvarchar](256) NOT NULL,
	[CategorySlug] [nvarchar](256) NOT NULL,
	[IsPublic] [bit] NOT NULL,
 CONSTRAINT [PK_carrot_ContentCategory] PRIMARY KEY NONCLUSTERED 
(
	[ContentCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_ContentComment](
	[ContentCommentID] [uniqueidentifier] NOT NULL,
	[Root_ContentID] [uniqueidentifier] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CommenterIP] [nvarchar](32) NOT NULL,
	[CommenterName] [nvarchar](256) NOT NULL,
	[CommenterEmail] [nvarchar](256) NOT NULL,
	[CommenterURL] [nvarchar](256) NOT NULL,
	[PostComment] [nvarchar](max) NULL,
	[IsApproved] [bit] NOT NULL,
	[IsSpam] [bit] NOT NULL,
 CONSTRAINT [PK_carrot_ContentComment] PRIMARY KEY NONCLUSTERED 
(
	[ContentCommentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_ContentSnippet](
	[ContentSnippetID] [uniqueidentifier] NOT NULL,
	[Root_ContentSnippetID] [uniqueidentifier] NOT NULL,
	[IsLatestVersion] [bit] NOT NULL,
	[EditUserId] [uniqueidentifier] NOT NULL,
	[EditDate] [datetime] NOT NULL,
	[ContentBody] [nvarchar](max) NULL,
 CONSTRAINT [PK_carrot_ContentSnippet] PRIMARY KEY CLUSTERED 
(
	[ContentSnippetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_ContentTag](
	[ContentTagID] [uniqueidentifier] NOT NULL,
	[SiteID] [uniqueidentifier] NOT NULL,
	[TagText] [nvarchar](256) NOT NULL,
	[TagSlug] [nvarchar](256) NOT NULL,
	[IsPublic] [bit] NOT NULL,
 CONSTRAINT [PK_carrot_ContentTag] PRIMARY KEY NONCLUSTERED 
(
	[ContentTagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_ContentType](
	[ContentTypeID] [uniqueidentifier] NOT NULL,
	[ContentTypeValue] [nvarchar](256) NOT NULL,
 CONSTRAINT [carrot_ContentType_PK] PRIMARY KEY CLUSTERED 
(
	[ContentTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_DataInfo](
	[DataInfoID] [uniqueidentifier] NOT NULL,
	[DataKey] [nvarchar](256) NOT NULL,
	[DataValue] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_carrot_DataInfo] PRIMARY KEY NONCLUSTERED 
(
	[DataInfoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_RootContent](
	[Root_ContentID] [uniqueidentifier] NOT NULL,
	[SiteID] [uniqueidentifier] NOT NULL,
	[Heartbeat_UserId] [uniqueidentifier] NULL,
	[EditHeartbeat] [datetime] NULL,
	[FileName] [nvarchar](256) NOT NULL,
	[PageActive] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[ContentTypeID] [uniqueidentifier] NOT NULL,
	[PageSlug] [nvarchar](256) NULL,
	[PageThumbnail] [nvarchar](128) NULL,
	[GoLiveDate] [datetime] NOT NULL,
	[RetireDate] [datetime] NOT NULL,
	[GoLiveDateLocal] [datetime] NOT NULL,
	[ShowInSiteNav] [bit] NOT NULL,
	[CreateUserId] [uniqueidentifier] NOT NULL,
	[ShowInSiteMap] [bit] NOT NULL,
	[BlockIndex] [bit] NOT NULL,
 CONSTRAINT [carrot_RootContent_PK] PRIMARY KEY CLUSTERED 
(
	[Root_ContentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_RootContentSnippet](
	[Root_ContentSnippetID] [uniqueidentifier] NOT NULL,
	[SiteID] [uniqueidentifier] NOT NULL,
	[ContentSnippetName] [nvarchar](256) NOT NULL,
	[ContentSnippetSlug] [nvarchar](128) NOT NULL,
	[CreateUserId] [uniqueidentifier] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[GoLiveDate] [datetime] NOT NULL,
	[RetireDate] [datetime] NOT NULL,
	[ContentSnippetActive] [bit] NOT NULL,
	[Heartbeat_UserId] [uniqueidentifier] NULL,
	[EditHeartbeat] [datetime] NULL,
 CONSTRAINT [PK_carrot_RootContentSnippet] PRIMARY KEY CLUSTERED 
(
	[Root_ContentSnippetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_SerialCache](
	[SerialCacheID] [uniqueidentifier] NOT NULL,
	[SiteID] [uniqueidentifier] NOT NULL,
	[ItemID] [uniqueidentifier] NOT NULL,
	[EditUserId] [uniqueidentifier] NOT NULL,
	[KeyType] [nvarchar](256) NULL,
	[SerializedData] [nvarchar](max) NULL,
	[EditDate] [datetime] NOT NULL,
 CONSTRAINT [carrot_SerialCache_PK] PRIMARY KEY CLUSTERED 
(
	[SerialCacheID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_Sites](
	[SiteID] [uniqueidentifier] NOT NULL,
	[MetaKeyword] [nvarchar](1024) NULL,
	[MetaDescription] [nvarchar](1024) NULL,
	[SiteName] [nvarchar](256) NULL,
	[MainURL] [nvarchar](128) NULL,
	[BlockIndex] [bit] NOT NULL,
	[SiteTagline] [nvarchar](1024) NULL,
	[SiteTitlebarPattern] [nvarchar](1024) NULL,
	[Blog_Root_ContentID] [uniqueidentifier] NULL,
	[Blog_FolderPath] [nvarchar](64) NULL,
	[Blog_CategoryPath] [nvarchar](64) NULL,
	[Blog_TagPath] [nvarchar](64) NULL,
	[Blog_DatePath] [nvarchar](64) NULL,
	[Blog_DatePattern] [nvarchar](32) NULL,
	[TimeZone] [nvarchar](128) NULL,
	[SendTrackbacks] [bit] NOT NULL,
	[AcceptTrackbacks] [bit] NOT NULL,
	[Blog_EditorPath] [nvarchar](64) NULL,
 CONSTRAINT [carrot_Sites_PK] PRIMARY KEY CLUSTERED 
(
	[SiteID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_TagContentMapping](
	[TagContentMappingID] [uniqueidentifier] NOT NULL,
	[ContentTagID] [uniqueidentifier] NOT NULL,
	[Root_ContentID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_carrot_TagContentMapping] PRIMARY KEY NONCLUSTERED 
(
	[TagContentMappingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_TextWidget](
	[TextWidgetID] [uniqueidentifier] NOT NULL,
	[SiteID] [uniqueidentifier] NOT NULL,
	[TextWidgetAssembly] [nvarchar](256) NOT NULL,
	[ProcessBody] [bit] NOT NULL,
	[ProcessPlainText] [bit] NOT NULL,
	[ProcessHTMLText] [bit] NOT NULL,
	[ProcessComment] [bit] NOT NULL,
	[ProcessSnippet] [bit] NOT NULL,
 CONSTRAINT [PK_carrot_TextWidget] PRIMARY KEY NONCLUSTERED 
(
	[TextWidgetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_TrackbackQueue](
	[TrackbackQueueID] [uniqueidentifier] NOT NULL,
	[Root_ContentID] [uniqueidentifier] NOT NULL,
	[TrackBackURL] [nvarchar](256) NOT NULL,
	[TrackBackResponse] [nvarchar](2048) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[TrackedBack] [bit] NOT NULL,
 CONSTRAINT [PK_carrot_TrackbackQueue] PRIMARY KEY NONCLUSTERED 
(
	[TrackbackQueueID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_UserData](
	[UserId] [uniqueidentifier] NOT NULL,
	[UserNickName] [nvarchar](64) NULL,
	[FirstName] [nvarchar](64) NULL,
	[LastName] [nvarchar](64) NULL,
	[UserBio] [nvarchar](max) NULL,
	[UserKey] [nvarchar](128) NULL,
 CONSTRAINT [PK_carrot_UserData] PRIMARY KEY NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_UserSiteMapping](
	[UserSiteMappingID] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[SiteID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [carrot_UserSiteMapping_PK] PRIMARY KEY CLUSTERED 
(
	[UserSiteMappingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_Widget](
	[Root_WidgetID] [uniqueidentifier] NOT NULL,
	[Root_ContentID] [uniqueidentifier] NOT NULL,
	[WidgetOrder] [int] NOT NULL,
	[PlaceholderName] [nvarchar](256) NOT NULL,
	[ControlPath] [nvarchar](1024) NOT NULL,
	[WidgetActive] [bit] NOT NULL,
	[GoLiveDate] [datetime] NOT NULL,
	[RetireDate] [datetime] NOT NULL,
 CONSTRAINT [PK_carrot_Widget] PRIMARY KEY CLUSTERED 
(
	[Root_WidgetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carrot_WidgetData](
	[WidgetDataID] [uniqueidentifier] NOT NULL,
	[Root_WidgetID] [uniqueidentifier] NOT NULL,
	[IsLatestVersion] [bit] NOT NULL,
	[EditDate] [datetime] NOT NULL,
	[ControlProperties] [nvarchar](max) NULL,
 CONSTRAINT [PK_carrot_WidgetData] PRIMARY KEY CLUSTERED 
(
	[WidgetDataID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[membership_Role](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.membership_Role] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[membership_User](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.membership_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[membership_UserClaim](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.membership_UserClaim] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[membership_UserLogin](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.membership_UserLogin] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[membership_UserRole](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.membership_UserRole] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_carrot_Content]
AS 

SELECT rc.Root_ContentID, rc.SiteID, rc.Heartbeat_UserId, rc.EditHeartbeat, rc.[FileName], rc.PageActive, rc.ShowInSiteNav, rc.ShowInSiteMap, rc.BlockIndex,
		rc.CreateUserId, rc.CreateDate, c.ContentID, c.Parent_ContentID, c.IsLatestVersion, c.TitleBar, c.NavMenuText, c.PageHead, 
		c.PageText, c.LeftPageText, c.RightPageText, c.NavOrder, c.EditUserId, c.CreditUserId, c.EditDate, c.TemplateFile, c.MetaKeyword, c.MetaDescription,
		cvh.VersionCount, ct.ContentTypeID, ct.ContentTypeValue, rc.PageSlug, rc.PageThumbnail, s.TimeZone,
		rc.RetireDate, rc.GoLiveDate, rc.GoLiveDateLocal,
		cast(case when rc.RetireDate <= GetUTCDate() then 1 else 0 end as bit) as IsRetired,
		cast(case when rc.GoLiveDate >= GetUTCDate() then 1 else 0 end as bit) as IsUnReleased
FROM [dbo].carrot_RootContent AS rc 
	INNER JOIN [dbo].carrot_Sites AS s ON rc.SiteID = s.SiteID 
	INNER JOIN [dbo].carrot_Content AS c ON rc.Root_ContentID = c.Root_ContentID 
	INNER JOIN [dbo].carrot_ContentType AS ct ON rc.ContentTypeID = ct.ContentTypeID
	INNER JOIN (SELECT COUNT(*) VersionCount, Root_ContentID 
				FROM [dbo].carrot_Content
				GROUP BY Root_ContentID 
				) cvh on rc.Root_ContentID = cvh.Root_ContentID

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_UserData]
AS 

SELECT mu.Id, mu.Email, mu.EmailConfirmed, mu.PasswordHash, mu.SecurityStamp, mu.PhoneNumber, mu.PhoneNumberConfirmed, 
		mu.TwoFactorEnabled, mu.LockoutEndDateUtc, mu.LockoutEnabled, mu.AccessFailedCount, mu.UserName, ud.UserId, ud.UserKey, 
		ud.UserNickName, ud.FirstName, ud.LastName, ud.UserBio
FROM dbo.membership_User AS mu 
LEFT JOIN carrot_UserData AS ud ON ud.UserKey = mu.Id

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_EditorURL]
AS 
-- select top 10 * from [vw_carrot_EditorURL]

select  d.SiteID, d.UserId, d.UserName, d.LoweredEmail, cc2.EditDate, 
		ISNULL(cc2.TheCount, 0) as UseCount, ISNULL(cc3.TheCount, 0) as PublicUseCount,
		'/'+d.Blog_FolderPath +'/'+ d.Blog_EditorPath +'/'+ d.UserName as UserUrl
from (
		select s.SiteID, s.Blog_FolderPath, s.Blog_EditorPath, m.UserId, m.UserName, lower(m.Email) as LoweredEmail
			from [dbo].vw_carrot_UserData m, [dbo].carrot_Sites s
		) as d
	left join (
			select v_cc.EditUserId, v_cc.SiteID, MAX(v_cc.EditDate) as EditDate, COUNT(ContentID) as TheCount
			from dbo.vw_carrot_Content v_cc
			where v_cc.IsLatestVersion = 1
			group by v_cc.EditUserId, v_cc.SiteID
			union
			select v_cc.CreditUserId, v_cc.SiteID, MAX(v_cc.EditDate) as EditDate, COUNT(ContentID) as TheCount
			from dbo.vw_carrot_Content v_cc
			where v_cc.IsLatestVersion = 1
				and v_cc.CreditUserId is not null
			group by v_cc.CreditUserId, v_cc.SiteID		
		
			) as cc2 on d.UserId = cc2.EditUserId
					and d.SiteID = cc2.SiteID
	left join (
			select v_cc.EditUserId, v_cc.SiteID, MAX(v_cc.EditDate) as EditDate, COUNT(ContentID) as TheCount
			from dbo.vw_carrot_Content v_cc
			where v_cc.IsLatestVersion = 1
				and v_cc.PageActive = 1 and v_cc.RetireDate >= GETUTCDATE() and v_cc.GoLiveDate <= GETUTCDATE() 
			group by v_cc.EditUserId, v_cc.SiteID
			union
			select v_cc.CreditUserId, v_cc.SiteID, MAX(v_cc.EditDate) as EditDate, COUNT(ContentID) as TheCount
			from dbo.vw_carrot_Content v_cc
			where v_cc.IsLatestVersion = 1 and v_cc.CreditUserId is not null
				and v_cc.PageActive = 1 and v_cc.RetireDate >= GETUTCDATE() and v_cc.GoLiveDate <= GETUTCDATE() 
			group by v_cc.CreditUserId, v_cc.SiteID	
			) as cc3 on d.UserId = cc3.EditUserId
					and d.SiteID = cc3.SiteID

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_TagURL]
AS 
-- select top 10 * from [vw_carrot_TagURL]

select  s.SiteID, cc.ContentTagID, cc.TagText, cc.IsPublic, cc2.EditDate, 
		ISNULL(cc2.TheCount, 0) as UseCount, ISNULL(cc3.TheCount, 0) as PublicUseCount,
		'/' + s.Blog_FolderPath + '/' + s.Blog_TagPath + '/' + cc.TagSlug as TagUrl
from [dbo].carrot_Sites as s 
	inner join [dbo].carrot_ContentTag as cc on s.SiteID = cc.SiteID
	left join (select m.ContentTagID, MAX(v_cc.EditDate) as EditDate, COUNT(m.Root_ContentID) as TheCount
				 from [dbo].vw_carrot_Content v_cc
					join [dbo].carrot_TagContentMapping m on v_cc.Root_ContentID = m.Root_ContentID
				 where v_cc.IsLatestVersion = 1
				 group by m.ContentTagID) as cc2 on cc.ContentTagID = cc2.ContentTagID

	left join (select m.ContentTagID, COUNT(m.Root_ContentID) as TheCount
				 from [dbo].vw_carrot_Content v_cc
					join [dbo].carrot_TagContentMapping m on v_cc.Root_ContentID = m.Root_ContentID
				 where v_cc.IsLatestVersion = 1
						and v_cc.PageActive = 1 and v_cc.RetireDate >= GETUTCDATE() and v_cc.GoLiveDate <= GETUTCDATE() 
				 group by m.ContentTagID) as cc3 on cc.ContentTagID = cc3.ContentTagID

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_CategoryURL]
AS 
-- select top 10 * from [vw_carrot_CategoryURL]

select  s.SiteID, cc.ContentCategoryID, cc.CategoryText, cc.IsPublic, cc2.EditDate, 
		ISNULL(cc2.TheCount, 0) as UseCount, ISNULL(cc3.TheCount, 0) as PublicUseCount,
		'/' + s.Blog_FolderPath + '/' + s.Blog_CategoryPath + '/' + cc.CategorySlug as CategoryUrl
from [dbo].carrot_Sites as s 
	inner join [dbo].carrot_ContentCategory as cc on s.SiteID = cc.SiteID
	left join (select m.ContentCategoryID, MAX(v_cc.EditDate) as EditDate, COUNT(m.Root_ContentID) as TheCount
				 from [dbo].vw_carrot_Content v_cc
					join [dbo].carrot_CategoryContentMapping m on v_cc.Root_ContentID = m.Root_ContentID
				 where v_cc.IsLatestVersion = 1
				 group by m.ContentCategoryID) as cc2 on cc.ContentCategoryID = cc2.ContentCategoryID

	left join (select m.ContentCategoryID, COUNT(m.Root_ContentID) as TheCount
				 from [dbo].vw_carrot_Content v_cc
					join [dbo].carrot_CategoryContentMapping m on v_cc.Root_ContentID = m.Root_ContentID
				 where v_cc.IsLatestVersion = 1
						and v_cc.PageActive = 1 and v_cc.RetireDate >= GETUTCDATE() and v_cc.GoLiveDate <= GETUTCDATE() 
				 group by m.ContentCategoryID) as cc3 on cc.ContentCategoryID = cc3.ContentCategoryID

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_ContentChild]
AS 

SELECT DISTINCT cc.SiteID, cc.Root_ContentID, cc.[FileName], 
          cc.RetireDate, cc.GoLiveDate, 
          cc.IsRetired, cc.IsUnReleased, 
          cp.Root_ContentID as Parent_ContentID, cp.[FileName] AS ParentFileName,
          cp.RetireDate AS ParentRetireDate, cp.GoLiveDate AS ParentGoLiveDate, 
          cp.IsRetired as IsParentRetired, cp.IsUnReleased as IsParentUnReleased
FROM dbo.vw_carrot_Content AS cc 
	INNER JOIN dbo.vw_carrot_Content AS cp ON cc.Parent_ContentID = cp.Root_ContentID
WHERE cp.IsLatestVersion = 1 AND cc.IsLatestVersion = 1
	AND cc.SiteID = cp.SiteID

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_Comment]
AS 

SELECT cc.ContentCommentID, cc.CreateDate, cc.CommenterIP, cc.CommenterName, cc.CommenterEmail, cc.CommenterURL, cc.PostComment, cc.IsApproved, cc.IsSpam, 
	c.Root_ContentID, c.SiteID, c.[FileName], c.PageHead, c.TitleBar, c.NavMenuText, c.IsRetired, c.IsUnReleased, c.RetireDate, c.GoLiveDate, 
	c.PageActive, c.ShowInSiteNav, c.ShowInSiteMap, c.BlockIndex, c.ContentTypeID, c.ContentTypeValue
FROM  dbo.carrot_ContentComment AS cc 
	INNER JOIN dbo.vw_carrot_Content AS c ON cc.Root_ContentID = c.Root_ContentID
WHERE (c.IsLatestVersion = 1)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_CategoryCounted]
AS 

SELECT cc.ContentCategoryID, cc.SiteID, cc.CategoryText, cc.CategorySlug, cc.IsPublic, ISNULL(cc2.TheCount, 0) AS UseCount
FROM dbo.carrot_ContentCategory AS cc 
LEFT JOIN
      (SELECT ContentCategoryID, COUNT(Root_ContentID) AS TheCount
        FROM dbo.carrot_CategoryContentMapping
        GROUP BY ContentCategoryID) AS cc2 ON cc.ContentCategoryID = cc2.ContentCategoryID

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_carrot_ContentSnippet]
AS 

SELECT csr.Root_ContentSnippetID, csr.SiteID, csr.ContentSnippetName, csr.ContentSnippetSlug, csr.CreateUserId, csr.CreateDate, 
	csr.ContentSnippetActive, cs.ContentSnippetID, cs.IsLatestVersion, cs.EditUserId, cs.EditDate, cs.ContentBody, 
	csr.Heartbeat_UserId, csr.EditHeartbeat, csr.GoLiveDate, csr.RetireDate,
	cast(case when csr.RetireDate < GetUTCDate() then 1 else 0 end as bit) as IsRetired,
	cast(case when csr.GoLiveDate > GetUTCDate() then 1 else 0 end as bit) as IsUnReleased,
	csvh.VersionCount
FROM carrot_RootContentSnippet AS csr 
	INNER JOIN carrot_ContentSnippet AS cs ON csr.Root_ContentSnippetID = cs.Root_ContentSnippetID
	INNER JOIN (SELECT COUNT(*) VersionCount, Root_ContentSnippetID 
				FROM [dbo].carrot_ContentSnippet
				GROUP BY Root_ContentSnippetID 
				) csvh on csr.Root_ContentSnippetID = csvh.Root_ContentSnippetID

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_EditHistory]
AS 

SELECT  rc.SiteID, c.ContentID, c.Root_ContentID, c.IsLatestVersion, c.TitleBar, c.NavMenuText, c.PageHead, c.CreditUserId, 
	c.EditDate, rc.CreateDate, rc.[FileName], ct.ContentTypeID, ct.ContentTypeValue, rc.PageActive, rc.GoLiveDate, rc.RetireDate, 
	c.EditUserId, m.UserName as EditUserName, m.Email as EditEmail, 
	rc.CreateUserId, m2.UserName as CreateUserName, m2.Email as CreateEmail
FROM [dbo].carrot_RootContent AS rc
	INNER JOIN [dbo].carrot_Content AS c ON rc.Root_ContentID = c.Root_ContentID 
	INNER JOIN [dbo].carrot_ContentType AS ct ON rc.ContentTypeID = ct.ContentTypeID
	INNER JOIN [dbo].carrot_UserData AS u ON c.EditUserId = u.UserId 
	INNER JOIN [dbo].membership_User AS m ON u.UserKey = m.Id
	INNER JOIN [dbo].carrot_UserData AS u2 ON rc.CreateUserId = u2.UserId 
	INNER JOIN [dbo].membership_User AS m2 ON u2.UserKey = m2.Id

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_TagCounted]
AS 

SELECT cc.ContentTagID, cc.SiteID, cc.TagText, cc.TagSlug, cc.IsPublic, ISNULL(cc2.TheCount, 0) AS UseCount
FROM dbo.carrot_ContentTag AS cc 
LEFT JOIN
      (SELECT ContentTagID, COUNT(Root_ContentID) AS TheCount
        FROM dbo.carrot_TagContentMapping
        GROUP BY ContentTagID) AS cc2 ON cc.ContentTagID = cc2.ContentTagID

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_carrot_TrackbackQueue]
AS 

SELECT tb.TrackbackQueueID, tb.TrackBackURL, tb.TrackBackResponse, tb.CreateDate, tb.ModifiedDate, tb.TrackedBack, c.Root_ContentID, c.PageActive, c.SiteID
FROM [dbo].carrot_TrackbackQueue AS tb
INNER JOIN [dbo].carrot_RootContent AS c ON tb.Root_ContentID = c.Root_ContentID

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_carrot_Widget]
AS 

SELECT w.Root_WidgetID, w.Root_ContentID, w.WidgetOrder, w.PlaceholderName, w.ControlPath, w.GoLiveDate, w.RetireDate, 
	cast(case when w.RetireDate < GetUTCDate() then 1 else 0 end as bit) as IsRetired,
	cast(case when w.GoLiveDate > GetUTCDate() then 1 else 0 end as bit) as IsUnReleased,
	w.WidgetActive, wd.WidgetDataID, wd.IsLatestVersion, wd.EditDate, wd.ControlProperties, cr.SiteID
FROM [dbo].carrot_Widget AS w 
INNER JOIN [dbo].carrot_WidgetData AS wd ON w.Root_WidgetID = wd.Root_WidgetID 
INNER JOIN [dbo].carrot_RootContent AS cr ON w.Root_ContentID = cr.Root_ContentID

GO
CREATE NONCLUSTERED INDEX [IDX_carrot_Content_EditUserId] ON [dbo].[carrot_Content]
(
	[EditUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_carrot_Content_Root_ContentID] ON [dbo].[carrot_Content]
(
	[Root_ContentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_carrot_RootContent_ContentTypeID] ON [dbo].[carrot_RootContent]
(
	[ContentTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_carrot_RootContent_CreateUserId] ON [dbo].[carrot_RootContent]
(
	[CreateUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_carrot_RootContent_SiteID] ON [dbo].[carrot_RootContent]
(
	[SiteID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[membership_Role]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[membership_User]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[membership_UserClaim]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[membership_UserLogin]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IX_RoleId] ON [dbo].[membership_UserRole]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[membership_UserRole]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[carrot_CategoryContentMapping] ADD  CONSTRAINT [DF_carrot_CategoryContentMapping_CategoryContentMappingID]  DEFAULT (newid()) FOR [CategoryContentMappingID]
GO
ALTER TABLE [dbo].[carrot_Content] ADD  CONSTRAINT [DF_carrot_Content_ContentID]  DEFAULT (newid()) FOR [ContentID]
GO
ALTER TABLE [dbo].[carrot_Content] ADD  CONSTRAINT [DF_carrot_Content_EditDate]  DEFAULT (getdate()) FOR [EditDate]
GO
ALTER TABLE [dbo].[carrot_ContentCategory] ADD  CONSTRAINT [DF_carrot_ContentCategory_ContentCategoryID]  DEFAULT (newid()) FOR [ContentCategoryID]
GO
ALTER TABLE [dbo].[carrot_ContentComment] ADD  CONSTRAINT [DF_carrot_ContentComment_ContentCommentID]  DEFAULT (newid()) FOR [ContentCommentID]
GO
ALTER TABLE [dbo].[carrot_ContentComment] ADD  CONSTRAINT [DF_carrot_ContentComment_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[carrot_ContentSnippet] ADD  CONSTRAINT [DF_carrot_ContentSnippet_ContentSnippetID]  DEFAULT (newid()) FOR [ContentSnippetID]
GO
ALTER TABLE [dbo].[carrot_ContentTag] ADD  CONSTRAINT [DF_carrot_ContentTag_ContentTagID]  DEFAULT (newid()) FOR [ContentTagID]
GO
ALTER TABLE [dbo].[carrot_ContentType] ADD  CONSTRAINT [DF_carrot_ContentType_ContentTypeID]  DEFAULT (newid()) FOR [ContentTypeID]
GO
ALTER TABLE [dbo].[carrot_DataInfo] ADD  CONSTRAINT [DF_carrot_DataInfo_DataInfoID]  DEFAULT (newid()) FOR [DataInfoID]
GO
ALTER TABLE [dbo].[carrot_RootContent] ADD  CONSTRAINT [DF_carrot_RootContent_Root_ContentID]  DEFAULT (newid()) FOR [Root_ContentID]
GO
ALTER TABLE [dbo].[carrot_RootContent] ADD  CONSTRAINT [DF_carrot_RootContent_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[carrot_RootContent] ADD  CONSTRAINT [DF_carrot_RootContent_GoLiveDate]  DEFAULT (getutcdate()) FOR [GoLiveDate]
GO
ALTER TABLE [dbo].[carrot_RootContent] ADD  CONSTRAINT [DF_carrot_RootContent_RetireDate]  DEFAULT (getutcdate()) FOR [RetireDate]
GO
ALTER TABLE [dbo].[carrot_RootContent] ADD  CONSTRAINT [DF_carrot_RootContent_GoLiveDateLocal]  DEFAULT (getutcdate()) FOR [GoLiveDateLocal]
GO
ALTER TABLE [dbo].[carrot_RootContentSnippet] ADD  CONSTRAINT [DF_carrot_RootContentSnippet_Root_ContentSnippetID]  DEFAULT (newid()) FOR [Root_ContentSnippetID]
GO
ALTER TABLE [dbo].[carrot_SerialCache] ADD  CONSTRAINT [DF_carrot_SerialCache_SerialCacheID]  DEFAULT (newid()) FOR [SerialCacheID]
GO
ALTER TABLE [dbo].[carrot_SerialCache] ADD  CONSTRAINT [DF_carrot_SerialCache_EditDate]  DEFAULT (getdate()) FOR [EditDate]
GO
ALTER TABLE [dbo].[carrot_Sites] ADD  CONSTRAINT [DF_carrot_Sites_SiteID]  DEFAULT (newid()) FOR [SiteID]
GO
ALTER TABLE [dbo].[carrot_TagContentMapping] ADD  CONSTRAINT [DF_carrot_TagContentMapping_TagContentMappingID]  DEFAULT (newid()) FOR [TagContentMappingID]
GO
ALTER TABLE [dbo].[carrot_TextWidget] ADD  CONSTRAINT [DF_carrot_TextWidget_TextWidgetID]  DEFAULT (newid()) FOR [TextWidgetID]
GO
ALTER TABLE [dbo].[carrot_TrackbackQueue] ADD  CONSTRAINT [DF_carrot_TrackbackQueue_TrackbackQueueID]  DEFAULT (newid()) FOR [TrackbackQueueID]
GO
ALTER TABLE [dbo].[carrot_TrackbackQueue] ADD  CONSTRAINT [DF_carrot_TrackbackQueue_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[carrot_UserSiteMapping] ADD  CONSTRAINT [DF_carrot_UserSiteMapping_UserSiteMappingID]  DEFAULT (newid()) FOR [UserSiteMappingID]
GO
ALTER TABLE [dbo].[carrot_Widget] ADD  CONSTRAINT [DF_carrot_Widget_Root_WidgetID]  DEFAULT (newid()) FOR [Root_WidgetID]
GO
ALTER TABLE [dbo].[carrot_WidgetData] ADD  CONSTRAINT [DF_carrot_WidgetData_WidgetDataID]  DEFAULT (newid()) FOR [WidgetDataID]
GO
ALTER TABLE [dbo].[carrot_WidgetData] ADD  CONSTRAINT [DF_carrot_WidgetData_EditDate]  DEFAULT (getdate()) FOR [EditDate]
GO
ALTER TABLE [dbo].[carrot_CategoryContentMapping]  WITH CHECK ADD  CONSTRAINT [FK_carrot_CategoryContentMapping_ContentCategoryID] FOREIGN KEY([ContentCategoryID])
REFERENCES [dbo].[carrot_ContentCategory] ([ContentCategoryID])
GO
ALTER TABLE [dbo].[carrot_CategoryContentMapping] CHECK CONSTRAINT [FK_carrot_CategoryContentMapping_ContentCategoryID]
GO
ALTER TABLE [dbo].[carrot_CategoryContentMapping]  WITH CHECK ADD  CONSTRAINT [FK_carrot_CategoryContentMapping_Root_ContentID] FOREIGN KEY([Root_ContentID])
REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
GO
ALTER TABLE [dbo].[carrot_CategoryContentMapping] CHECK CONSTRAINT [FK_carrot_CategoryContentMapping_Root_ContentID]
GO
ALTER TABLE [dbo].[carrot_Content]  WITH CHECK ADD  CONSTRAINT [carrot_Content_CreditUserId_FK] FOREIGN KEY([CreditUserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])
GO
ALTER TABLE [dbo].[carrot_Content] CHECK CONSTRAINT [carrot_Content_CreditUserId_FK]
GO
ALTER TABLE [dbo].[carrot_Content]  WITH CHECK ADD  CONSTRAINT [carrot_Content_EditUserId_FK] FOREIGN KEY([EditUserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])
GO
ALTER TABLE [dbo].[carrot_Content] CHECK CONSTRAINT [carrot_Content_EditUserId_FK]
GO
ALTER TABLE [dbo].[carrot_Content]  WITH CHECK ADD  CONSTRAINT [carrot_RootContent_carrot_Content_FK] FOREIGN KEY([Root_ContentID])
REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
GO
ALTER TABLE [dbo].[carrot_Content] CHECK CONSTRAINT [carrot_RootContent_carrot_Content_FK]
GO
ALTER TABLE [dbo].[carrot_ContentCategory]  WITH CHECK ADD  CONSTRAINT [FK_carrot_ContentCategory_SiteID] FOREIGN KEY([SiteID])
REFERENCES [dbo].[carrot_Sites] ([SiteID])
GO
ALTER TABLE [dbo].[carrot_ContentCategory] CHECK CONSTRAINT [FK_carrot_ContentCategory_SiteID]
GO
ALTER TABLE [dbo].[carrot_ContentComment]  WITH CHECK ADD  CONSTRAINT [FK_carrot_ContentComment_Root_ContentID] FOREIGN KEY([Root_ContentID])
REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
GO
ALTER TABLE [dbo].[carrot_ContentComment] CHECK CONSTRAINT [FK_carrot_ContentComment_Root_ContentID]
GO
ALTER TABLE [dbo].[carrot_ContentSnippet]  WITH CHECK ADD  CONSTRAINT [FK_carrot_ContentSnippet_Root_ContentSnippetID] FOREIGN KEY([Root_ContentSnippetID])
REFERENCES [dbo].[carrot_RootContentSnippet] ([Root_ContentSnippetID])
GO
ALTER TABLE [dbo].[carrot_ContentSnippet] CHECK CONSTRAINT [FK_carrot_ContentSnippet_Root_ContentSnippetID]
GO
ALTER TABLE [dbo].[carrot_ContentTag]  WITH CHECK ADD  CONSTRAINT [FK_carrot_ContentTag_SiteID] FOREIGN KEY([SiteID])
REFERENCES [dbo].[carrot_Sites] ([SiteID])
GO
ALTER TABLE [dbo].[carrot_ContentTag] CHECK CONSTRAINT [FK_carrot_ContentTag_SiteID]
GO
ALTER TABLE [dbo].[carrot_RootContent]  WITH CHECK ADD  CONSTRAINT [carrot_ContentType_carrot_RootContent_FK] FOREIGN KEY([ContentTypeID])
REFERENCES [dbo].[carrot_ContentType] ([ContentTypeID])
GO
ALTER TABLE [dbo].[carrot_RootContent] CHECK CONSTRAINT [carrot_ContentType_carrot_RootContent_FK]
GO
ALTER TABLE [dbo].[carrot_RootContent]  WITH CHECK ADD  CONSTRAINT [carrot_RootContent_CreateUserId_FK] FOREIGN KEY([CreateUserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])
GO
ALTER TABLE [dbo].[carrot_RootContent] CHECK CONSTRAINT [carrot_RootContent_CreateUserId_FK]
GO
ALTER TABLE [dbo].[carrot_RootContent]  WITH CHECK ADD  CONSTRAINT [carrot_Sites_carrot_RootContent_FK] FOREIGN KEY([SiteID])
REFERENCES [dbo].[carrot_Sites] ([SiteID])
GO
ALTER TABLE [dbo].[carrot_RootContent] CHECK CONSTRAINT [carrot_Sites_carrot_RootContent_FK]
GO
ALTER TABLE [dbo].[carrot_RootContentSnippet]  WITH CHECK ADD  CONSTRAINT [FK_carrot_RootContentSnippet_SiteID] FOREIGN KEY([SiteID])
REFERENCES [dbo].[carrot_Sites] ([SiteID])
GO
ALTER TABLE [dbo].[carrot_RootContentSnippet] CHECK CONSTRAINT [FK_carrot_RootContentSnippet_SiteID]
GO
ALTER TABLE [dbo].[carrot_TagContentMapping]  WITH CHECK ADD  CONSTRAINT [FK_carrot_TagContentMapping_ContentTagID] FOREIGN KEY([ContentTagID])
REFERENCES [dbo].[carrot_ContentTag] ([ContentTagID])
GO
ALTER TABLE [dbo].[carrot_TagContentMapping] CHECK CONSTRAINT [FK_carrot_TagContentMapping_ContentTagID]
GO
ALTER TABLE [dbo].[carrot_TagContentMapping]  WITH CHECK ADD  CONSTRAINT [FK_carrot_TagContentMapping_Root_ContentID] FOREIGN KEY([Root_ContentID])
REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
GO
ALTER TABLE [dbo].[carrot_TagContentMapping] CHECK CONSTRAINT [FK_carrot_TagContentMapping_Root_ContentID]
GO
ALTER TABLE [dbo].[carrot_TextWidget]  WITH CHECK ADD  CONSTRAINT [FK_carrot_TextWidget_SiteID] FOREIGN KEY([SiteID])
REFERENCES [dbo].[carrot_Sites] ([SiteID])
GO
ALTER TABLE [dbo].[carrot_TextWidget] CHECK CONSTRAINT [FK_carrot_TextWidget_SiteID]
GO
ALTER TABLE [dbo].[carrot_TrackbackQueue]  WITH CHECK ADD  CONSTRAINT [FK_carrot_TrackbackQueue_Root_ContentID] FOREIGN KEY([Root_ContentID])
REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
GO
ALTER TABLE [dbo].[carrot_TrackbackQueue] CHECK CONSTRAINT [FK_carrot_TrackbackQueue_Root_ContentID]
GO
ALTER TABLE [dbo].[carrot_UserData]  WITH CHECK ADD  CONSTRAINT [carrot_UserData_UserKey] FOREIGN KEY([UserKey])
REFERENCES [dbo].[membership_User] ([Id])
GO
ALTER TABLE [dbo].[carrot_UserData] CHECK CONSTRAINT [carrot_UserData_UserKey]
GO
ALTER TABLE [dbo].[carrot_UserSiteMapping]  WITH CHECK ADD  CONSTRAINT [aspnet_Users_carrot_UserSiteMapping_FK] FOREIGN KEY([UserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])
GO
ALTER TABLE [dbo].[carrot_UserSiteMapping] CHECK CONSTRAINT [aspnet_Users_carrot_UserSiteMapping_FK]
GO
ALTER TABLE [dbo].[carrot_UserSiteMapping]  WITH CHECK ADD  CONSTRAINT [carrot_Sites_carrot_UserSiteMapping_FK] FOREIGN KEY([SiteID])
REFERENCES [dbo].[carrot_Sites] ([SiteID])
GO
ALTER TABLE [dbo].[carrot_UserSiteMapping] CHECK CONSTRAINT [carrot_Sites_carrot_UserSiteMapping_FK]
GO
ALTER TABLE [dbo].[carrot_Widget]  WITH CHECK ADD  CONSTRAINT [carrot_RootContent_carrot_Widget_FK] FOREIGN KEY([Root_ContentID])
REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
GO
ALTER TABLE [dbo].[carrot_Widget] CHECK CONSTRAINT [carrot_RootContent_carrot_Widget_FK]
GO
ALTER TABLE [dbo].[carrot_WidgetData]  WITH CHECK ADD  CONSTRAINT [carrot_WidgetData_Root_WidgetID_FK] FOREIGN KEY([Root_WidgetID])
REFERENCES [dbo].[carrot_Widget] ([Root_WidgetID])
GO
ALTER TABLE [dbo].[carrot_WidgetData] CHECK CONSTRAINT [carrot_WidgetData_Root_WidgetID_FK]
GO
ALTER TABLE [dbo].[membership_UserClaim]  WITH CHECK ADD  CONSTRAINT [FK_dbo.membership_UserClaim_dbo.membership_User_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[membership_User] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[membership_UserClaim] CHECK CONSTRAINT [FK_dbo.membership_UserClaim_dbo.membership_User_UserId]
GO
ALTER TABLE [dbo].[membership_UserLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.membership_UserLogin_dbo.membership_User_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[membership_User] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[membership_UserLogin] CHECK CONSTRAINT [FK_dbo.membership_UserLogin_dbo.membership_User_UserId]
GO
ALTER TABLE [dbo].[membership_UserRole]  WITH CHECK ADD  CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_Role_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[membership_Role] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[membership_UserRole] CHECK CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_Role_RoleId]
GO
ALTER TABLE [dbo].[membership_UserRole]  WITH CHECK ADD  CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_User_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[membership_User] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[membership_UserRole] CHECK CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_User_UserId]
GO
--================================================================================

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[carrot_BlogDateFilenameUpdate]
    @SiteID uniqueidentifier
    
/*

exec [carrot_BlogDateFilenameUpdate] '3BD253EA-AC65-4EB6-A4E7-BB097C2255A0'

*/    
    
AS BEGIN

SET NOCOUNT ON

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0
    
    DECLARE @DatePattern nvarchar(50)
    SELECT  @DatePattern = (select top 1 ct.Blog_DatePattern from dbo.carrot_Sites (nolock) ct where ct.SiteID = @SiteID)

	DECLARE @blogType uniqueidentifier
	SELECT  @blogType = (select top 1 ct.ContentTypeID from dbo.carrot_ContentType (nolock) ct where ct.ContentTypeValue = 'BlogEntry')

	DECLARE @tblTimeSlugs TABLE(
		GoLiveDateLocal datetime,
		URLBase nvarchar(256)
	)

	insert into @tblTimeSlugs(GoLiveDateLocal)
		select distinct rc.GoLiveDateLocal
		from dbo.[carrot_RootContent] as rc (nolock)
		where rc.SiteID = @SiteID
			and rc.ContentTypeID = @blogType

	IF (@DatePattern = 'yyyy/M/d' ) BEGIN
		update @tblTimeSlugs
		set URLBase = REPLACE(CONVERT(NVARCHAR(20), GoLiveDateLocal, 111), '/0', '/')
	END

	IF (@DatePattern = 'yyyy/MM' ) BEGIN
		update @tblTimeSlugs
		set URLBase = SUBSTRING(CONVERT(NVARCHAR(20), GoLiveDateLocal, 111), 1, 7)
	END

	IF (@DatePattern = 'yyyy/MMMM' ) BEGIN
		update @tblTimeSlugs
		set URLBase = CAST(YEAR(GoLiveDateLocal) as nvarchar(20)) +'/'+ DATENAME(MONTH, GoLiveDateLocal)
	END

	IF (@DatePattern = 'yyyy' ) BEGIN
		update @tblTimeSlugs
		set URLBase = CAST(YEAR(GoLiveDateLocal) as nvarchar(20))
	END

	IF (ISNULL(@DatePattern, 'yyyy/MM/dd') = 'yyyy/MM/dd' ) 
			OR EXISTS(select * from @tblTimeSlugs where URLBase is null or len(URLBase) < 1) BEGIN
		update @tblTimeSlugs
		set URLBase = CONVERT(NVARCHAR(20), GoLiveDateLocal, 111)
	END

    IF ( @@TRANCOUNT = 0 ) BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END ELSE
        SET @TranStarted = 0

		update rc
		set [FileName] = replace('/'+ s.URLBase +'/' + ISNULL(rc.PageSlug, cast(Root_ContentID as nvarchar(64))) , '//','/')
		from dbo.[carrot_RootContent] rc
			join @tblTimeSlugs s on rc.GoLiveDateLocal = s.GoLiveDateLocal
		where rc.SiteID = @SiteID
			AND rc.ContentTypeID = @blogType

    IF ( @@ERROR <> 0 ) BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF ( @TranStarted = 1 ) BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF ( @TranStarted = 1 ) BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END

GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[carrot_BlogMonthlyTallies]
    @SiteID uniqueidentifier,
    @ActiveOnly bit,    
    @TakeTop int = 10

/*

exec [carrot_BlogMonthlyTallies] '3BD253EA-AC65-4EB6-A4E7-BB097C2255A0', 1, 16

exec [carrot_BlogMonthlyTallies] '3BD253EA-AC65-4EB6-A4E7-BB097C2255A0', 0, 16

*/

AS BEGIN

SET NOCOUNT ON

	DECLARE @UTCDateTime Datetime
	SET @UTCDateTime = GetUTCDate()
	
	DECLARE @blogType uniqueidentifier
	SELECT  @blogType = (select top 1 ct.ContentTypeID from dbo.carrot_ContentType (nolock) ct where ct.ContentTypeValue = 'BlogEntry')

	DECLARE @tblTallies TABLE(
		RowID int identity(1,1),
		SiteID uniqueidentifier,
		ContentCount int,
		DateMonth date,
		DateSlug nvarchar(64)
	)
	
	insert into @tblTallies(SiteID, ContentCount, DateMonth, DateSlug)
		SELECT SiteID, COUNT(Root_ContentID) AS ContentCount, DateMonth, DateSlug
		FROM   (SELECT Root_ContentID, SiteID, ContentTypeID, 
					CONVERT(datetime, CONVERT(nvarchar(100), GoLiveDateLocal, 112)) AS DateMonth, 
					DATENAME(MONTH, GoLiveDateLocal) + ' ' + CAST(YEAR(GoLiveDateLocal) as nvarchar(100)) AS DateSlug
			FROM (SELECT Root_ContentID, SiteID, ContentTypeID, (GoLiveDateLocal - DAY(GoLiveDateLocal) + 1) as GoLiveDateLocal
				FROM [dbo].[carrot_RootContent] (nolock)
				WHERE SiteID = @SiteID
					AND (PageActive = 1 OR @ActiveOnly = 0)
					AND (GoLiveDate < @UTCDateTime OR @ActiveOnly = 0)
					AND (RetireDate > @UTCDateTime OR @ActiveOnly = 0)
					AND ContentTypeID = @blogType ) AS Y) AS Z

		GROUP BY SiteID, DateMonth, DateSlug
		ORDER BY DateMonth DESC

	SELECT * FROM @tblTallies WHERE RowID <= @TakeTop ORDER BY RowID

    RETURN(0)

END

GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[carrot_UpdateGoLiveLocal]
    @SiteID uniqueidentifier,
    @xmlDocument xml = '<rows />'
AS BEGIN

SET NOCOUNT ON

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF ( @@TRANCOUNT = 0 ) BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END ELSE BEGIN
        SET @TranStarted = 0
	END

		DECLARE @blogType uniqueidentifier
		SELECT  @blogType = (select top 1 ct.ContentTypeID from dbo.carrot_ContentType (nolock) ct where ct.ContentTypeValue = 'BlogEntry')

		DECLARE @tblContent TABLE
		(
		  GoLiveDate datetime,
		  GoLiveDateLocal datetime
		)

		DECLARE @tblBlogs TABLE
		(
		  GoLiveDate datetime,
		  GoLiveDateLocal datetime,
		  PostPrefix nvarchar(256)  
		)

		INSERT INTO @tblContent(GoLiveDate, GoLiveDateLocal)
		SELECT
			ref.value ('GoLiveDate[1]', 'datetime') as GoLiveDate,
			ref.value ('GoLiveDateLocal[1]', 'datetime') as GoLiveDateLocal
		FROM @xmlDocument.nodes ('//ContentLocalTime') T(ref);

		INSERT INTO @tblBlogs(GoLiveDate, GoLiveDateLocal, PostPrefix)
		SELECT
			ref.value ('GoLiveDate[1]', 'datetime') as GoLiveDate,
			ref.value ('GoLiveDateLocal[1]', 'datetime') as GoLiveDateLocal,
			ref.value ('PostPrefix[1]', 'nvarchar(256)') as PostPrefix
		FROM @xmlDocument.nodes ('//BlogPostPageUrl') T(ref);

		update @tblBlogs
			set PostPrefix = cast(DATEPART(YEAR, GoLiveDateLocal) as varchar(32)) + '/' + cast(DATEPART(MONTH, GoLiveDateLocal) as varchar(32)) + '/' + cast(DATEPART(DAY, GoLiveDateLocal) as varchar(32)) + '/'
		where PostPrefix is null or len(PostPrefix) < 3

		UPDATE rc
			SET GoLiveDateLocal = c.GoLiveDateLocal
		FROM [dbo].[carrot_RootContent] rc
			JOIN @tblContent c on rc.GoLiveDate = c.GoLiveDate
		WHERE rc.SiteID = @SiteID

		UPDATE rc
			SET [FileName] = replace(b.PostPrefix + '/' + rc.PageSlug, '//',  '/')
		FROM [dbo].[carrot_RootContent] rc
			JOIN @tblBlogs b on rc.GoLiveDate = b.GoLiveDate
		WHERE rc.SiteID = @SiteID 
				and rc.ContentTypeID = @blogType

	IF ( @@ERROR <> 0 ) BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF ( @TranStarted = 1 ) BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF ( @TranStarted = 1 ) BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END

GO

--================================================================================

GO

declare @GrpAdminID uniqueidentifier
declare @GrpEditID uniqueidentifier
declare @GrpUserID uniqueidentifier

IF ((select count([Id]) from [dbo].[membership_Role] where [Name] = N'CarrotCMS Administrators') < 1) BEGIN

	INSERT [dbo].[membership_Role] ([Id], [Name])
		 VALUES (lower(NewID()), N'CarrotCMS Administrators')
	INSERT [dbo].[membership_Role] ([Id], [Name])
		  VALUES (lower(NewID()), N'CarrotCMS Editors')
	INSERT [dbo].[membership_Role] ([Id], [Name])
		  VALUES (lower(NewID()), N'CarrotCMS Users')

END

set @GrpAdminID = (select top 1 [Id] from [dbo].[membership_Role] where [Name] = N'CarrotCMS Administrators' )
set @GrpEditID = (select top 1 [Id] from [dbo].[membership_Role] where [Name] = N'CarrotCMS Editors' )
set @GrpUserID = (select top 1 [Id] from [dbo].[membership_Role] where [Name] = N'CarrotCMS Users' )

select [Id], [Name] from [dbo].[membership_Role] where [Id] in (@GrpAdminID, @GrpEditID, @GrpUserID)

GO

--=======================================

GO

IF ((select count(*) from [dbo].[carrot_ContentType] where [ContentTypeValue] = N'ContentEntry') < 1) BEGIN

	insert into [dbo].[carrot_ContentType]([ContentTypeValue])
	values('BlogEntry')

	insert into [dbo].[carrot_ContentType]([ContentTypeValue])
	values('ContentEntry')

END

GO

--=======================================

GO

if (not exists(select * from [dbo].[carrot_DataInfo]  where [DataKey] = 'DBSchema')) begin

	INSERT [dbo].[carrot_DataInfo] ([DataInfoID], [DataKey], [DataValue]) VALUES (NewID(), N'DBSchema', N'20200915')

end

GO
