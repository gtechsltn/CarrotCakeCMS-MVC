GO

ALTER TABLE [dbo].[carrot_Content] DROP CONSTRAINT [carrot_Content_CreditUserId_FK]
ALTER TABLE [dbo].[carrot_Content] DROP CONSTRAINT [carrot_Content_EditUserId_FK]

GO

ALTER TABLE [dbo].[carrot_Content]  WITH CHECK ADD  CONSTRAINT [carrot_Content_CreditUserId_FK] FOREIGN KEY([CreditUserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])

ALTER TABLE [dbo].[carrot_Content]  WITH CHECK ADD  CONSTRAINT [carrot_Content_EditUserId_FK] FOREIGN KEY([EditUserId])
REFERENCES [dbo].[carrot_UserData] ([UserId])

GO

ALTER TABLE [dbo].[carrot_Content] CHECK CONSTRAINT [carrot_Content_CreditUserId_FK]
ALTER TABLE [dbo].[carrot_Content] CHECK CONSTRAINT [carrot_Content_EditUserId_FK]

GO

SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET NUMERIC_ROUNDABORT OFF
SET QUOTED_IDENTIFIER ON

GO

ALTER PROCEDURE [dbo].[carrot_UpdateGoLiveLocal]
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

SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET NUMERIC_ROUNDABORT OFF
SET QUOTED_IDENTIFIER ON

GO

ALTER PROCEDURE [dbo].[carrot_BlogDateFilenameUpdate]
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
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET NUMERIC_ROUNDABORT OFF
SET QUOTED_IDENTIFIER ON

GO

ALTER PROCEDURE [dbo].[carrot_BlogMonthlyTallies]
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
