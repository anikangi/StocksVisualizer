USE [master]
GO
/****** Object:  Database [NeeksDB]    Script Date: 7/21/2022 11:52:42 PM ******/
CREATE DATABASE [NeeksDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'NeeksDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\NeeksDB.mdf' , SIZE = 794624KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'NeeksDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\NeeksDB_log.ldf' , SIZE = 2367488KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [NeeksDB] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [NeeksDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [NeeksDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [NeeksDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [NeeksDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [NeeksDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [NeeksDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [NeeksDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [NeeksDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [NeeksDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [NeeksDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [NeeksDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [NeeksDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [NeeksDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [NeeksDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [NeeksDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [NeeksDB] SET  DISABLE_BROKER 
GO
ALTER DATABASE [NeeksDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [NeeksDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [NeeksDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [NeeksDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [NeeksDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [NeeksDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [NeeksDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [NeeksDB] SET RECOVERY FULL 
GO
ALTER DATABASE [NeeksDB] SET  MULTI_USER 
GO
ALTER DATABASE [NeeksDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [NeeksDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [NeeksDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [NeeksDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [NeeksDB] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'NeeksDB', N'ON'
GO
ALTER DATABASE [NeeksDB] SET QUERY_STORE = OFF
GO
USE [NeeksDB]
GO
/****** Object:  Table [dbo].[MinuteData]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MinuteData](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Average] [float] NOT NULL,
	[Volume] [bigint] NOT NULL,
	[AmtOfTrades] [bigint] NOT NULL,
	[StockID] [bigint] NOT NULL,
 CONSTRAINT [PK_MinuteData] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Stock]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Stock](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Symbol] [nvarchar](10) NOT NULL,
	[Company] [nvarchar](255) NOT NULL,
	[Exchange] [nvarchar](200) NULL,
	[Industry] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
	[IssueType] [nvarchar](10) NULL,
	[SectorID] [bigint] NULL,
	[Employees] [int] NULL,
	[City] [nvarchar](50) NULL,
	[State] [nvarchar](50) NULL,
	[Country] [nvarchar](50) NULL,
	[LastUpdated_MinData] [datetime] NULL,
	[IsDisabled] [bit] NULL,
	[PercentData_LastDay] [decimal](5, 2) NULL,
	[PercentData_SamplePeriod] [decimal](5, 2) NULL,
	[IsTop4000] [bit] NULL,
 CONSTRAINT [PK_Stock] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[LastUpdatedMinData_V]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[LastUpdatedMinData_V]
AS
SELECT s.ID, s.Symbol, MAX(md.Date) AS LastUpdatedTime
FROM     dbo.Stock AS s INNER JOIN
                  dbo.MinuteData AS md ON s.ID = md.StockID
GROUP BY s.ID, s.Symbol
GO
/****** Object:  Table [dbo].[MinuteDataTester]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MinuteDataTester](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Average] [float] NULL,
	[Volume] [bigint] NOT NULL,
	[AmtOfTrades] [bigint] NOT NULL,
	[StockID] [bigint] NOT NULL,
 CONSTRAINT [PK_MinuteDataTester] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[LastUpdatedMinData_VTester]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[LastUpdatedMinData_VTester]
AS
SELECT s.ID, s.Symbol, MAX(md.Date) AS LastUpdatedTime
FROM     dbo.Stock AS s INNER JOIN
                  dbo.MinuteDataTester AS md ON s.ID = md.StockID
GROUP BY s.ID, s.Symbol
GO
/****** Object:  Table [dbo].[Sector]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sector](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Sector] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HistoricalData]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistoricalData](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[StockID] [bigint] NOT NULL,
	[Date] [date] NOT NULL,
	[Open] [float] NOT NULL,
	[Close] [float] NOT NULL,
	[High] [float] NOT NULL,
	[Low] [float] NOT NULL,
	[Volume] [bigint] NOT NULL,
	[MarketCap] [bigint] NULL,
 CONSTRAINT [PK_HistoricalData] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MarketCapitalization]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MarketCapitalization](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[StockID] [bigint] NOT NULL,
	[MarketCap] [bigint] NOT NULL,
	[Date] [date] NOT NULL,
 CONSTRAINT [PK_MarketCapitalization] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SampleStats]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SampleStats](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[StockID] [bigint] NOT NULL,
	[StDev] [float] NULL,
	[Date] [datetime] NOT NULL,
	[Total_AmtOfTrades] [bigint] NULL,
 CONSTRAINT [PK_Stats] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[Stocks_V]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Stocks_V]
AS
SELECT TOP (100) PERCENT s.ID AS StockID, s.Symbol, s.Company, se.Name AS Sector, s.Exchange, s.Industry, s.Description, s.LastUpdated_MinData, s.IsDisabled, m.MarketCap, m.Date AS Last_Updated_MC, MAX(sst.Date) 
                  AS MostRecent_Stats, MAX(h.Date) AS MostRecent_DailyData
FROM     dbo.Stock AS s LEFT OUTER JOIN
                  dbo.HistoricalData AS h ON s.ID = h.StockID LEFT OUTER JOIN
                  dbo.MarketCapitalization AS m ON s.ID = m.StockID LEFT OUTER JOIN
                  dbo.Sector AS se ON s.SectorID = se.ID LEFT OUTER JOIN
                  dbo.SampleStats AS sst ON s.ID = sst.StockID
WHERE  (s.IsTop4000 = 'true')
GROUP BY s.ID, s.Symbol, s.Company, se.Name, s.Exchange, s.Industry, s.Description, s.LastUpdated_MinData, s.IsDisabled, m.MarketCap, m.Date
GO
/****** Object:  View [dbo].[Stocks_V_Old]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Stocks_V_Old]
AS
SELECT TOP (100) PERCENT s.ID, s.Symbol, s.Company, s.Exchange, s.Industry, s.Description, s.IssueType, s.SectorID, s.Employees, s.City, s.State, s.Country, MAX(d.Date) AS LastUpdated_DailyData, s.LastUpdated_MinData, s.IsDisabled
FROM     dbo.Stock AS s LEFT OUTER JOIN
                  dbo.HistoricalData AS d ON s.ID = d.StockID
GROUP BY s.ID, s.Symbol, s.Company, s.Exchange, s.Industry, s.Description, s.IssueType, s.SectorID, s.Employees, s.City, s.State, s.Country, s.LastUpdated_MinData, s.IsDisabled
GO
/****** Object:  Table [dbo].[MEXSectorData]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MEXSectorData](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Time] [datetime] NOT NULL,
	[SectorId] [int] NOT NULL,
	[Value] [float] NOT NULL,
 CONSTRAINT [PK_SectorData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MEXSector]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MEXSector](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[SectorAssetClassId] [int] NULL,
 CONSTRAINT [PK_MEXSector] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[SectorData_V]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[SectorData_V]
AS
SELECT TOP (100) PERCENT sd.Id, sd.Time, sd.Value, s.Name AS Sector, sd.SectorId
FROM     dbo.MEXSectorData AS sd LEFT OUTER JOIN
                  dbo.MEXSector AS s ON sd.SectorId = s.Id
GO
/****** Object:  Table [dbo].[Exchange]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Exchange](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
	[LongName] [nvarchar](200) NOT NULL,
	[MIC] [nvarchar](10) NULL,
	[TapeId] [nvarchar](5) NULL,
 CONSTRAINT [PK_Exchange] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HistoricalData2]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistoricalData2](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[StockID] [bigint] NOT NULL,
	[Date] [date] NOT NULL,
	[Open] [float] NOT NULL,
	[Close] [float] NOT NULL,
	[High] [float] NOT NULL,
	[Low] [float] NOT NULL,
	[Volume] [bigint] NOT NULL,
 CONSTRAINT [PK_HistoricalData2] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Holidays]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Holidays](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Year] [int] NOT NULL,
	[Date] [date] NOT NULL,
	[EarlyClose] [bit] NOT NULL,
	[Name] [varchar](255) NULL,
 CONSTRAINT [PK_Holidays] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [idx_HistoricalData_Time]    Script Date: 7/21/2022 11:52:42 PM ******/
CREATE NONCLUSTERED INDEX [idx_HistoricalData_Time] ON [dbo].[HistoricalData]
(
	[Date] ASC
)
INCLUDE([StockID],[Open],[Close],[High],[Low],[Volume],[MarketCap]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_SectorData_Time]    Script Date: 7/21/2022 11:52:42 PM ******/
CREATE NONCLUSTERED INDEX [idx_SectorData_Time] ON [dbo].[MEXSectorData]
(
	[Time] ASC
)
INCLUDE([SectorId],[Value]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_MinuteData_Time]    Script Date: 7/21/2022 11:52:42 PM ******/
CREATE NONCLUSTERED INDEX [idx_MinuteData_Time] ON [dbo].[MinuteData]
(
	[Date] ASC
)
INCLUDE([ID],[Average],[Volume],[AmtOfTrades],[StockID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Holidays] ADD  CONSTRAINT [DF_Holidays_EarlyClose]  DEFAULT ((0)) FOR [EarlyClose]
GO
ALTER TABLE [dbo].[SampleStats] ADD  CONSTRAINT [DF_Table_1_SD_Last_Updated]  DEFAULT (getdate()) FOR [Date]
GO
ALTER TABLE [dbo].[HistoricalData]  WITH CHECK ADD  CONSTRAINT [FK_HistoricalData_Stock] FOREIGN KEY([StockID])
REFERENCES [dbo].[Stock] ([ID])
GO
ALTER TABLE [dbo].[HistoricalData] CHECK CONSTRAINT [FK_HistoricalData_Stock]
GO
ALTER TABLE [dbo].[HistoricalData2]  WITH CHECK ADD  CONSTRAINT [FK_HistoricalData_Stock2] FOREIGN KEY([StockID])
REFERENCES [dbo].[Stock] ([ID])
GO
ALTER TABLE [dbo].[HistoricalData2] CHECK CONSTRAINT [FK_HistoricalData_Stock2]
GO
ALTER TABLE [dbo].[MarketCapitalization]  WITH CHECK ADD  CONSTRAINT [FK_MarketCapitalization_Stock] FOREIGN KEY([StockID])
REFERENCES [dbo].[Stock] ([ID])
GO
ALTER TABLE [dbo].[MarketCapitalization] CHECK CONSTRAINT [FK_MarketCapitalization_Stock]
GO
ALTER TABLE [dbo].[MEXSectorData]  WITH CHECK ADD  CONSTRAINT [FK_MEXSectorData_MEXSector] FOREIGN KEY([SectorId])
REFERENCES [dbo].[MEXSector] ([Id])
GO
ALTER TABLE [dbo].[MEXSectorData] CHECK CONSTRAINT [FK_MEXSectorData_MEXSector]
GO
ALTER TABLE [dbo].[MinuteData]  WITH CHECK ADD  CONSTRAINT [FK_MinuteData_Stock] FOREIGN KEY([StockID])
REFERENCES [dbo].[Stock] ([ID])
GO
ALTER TABLE [dbo].[MinuteData] CHECK CONSTRAINT [FK_MinuteData_Stock]
GO
ALTER TABLE [dbo].[MinuteDataTester]  WITH CHECK ADD  CONSTRAINT [FK_MinuteDataTester_Stock] FOREIGN KEY([StockID])
REFERENCES [dbo].[Stock] ([ID])
GO
ALTER TABLE [dbo].[MinuteDataTester] CHECK CONSTRAINT [FK_MinuteDataTester_Stock]
GO
ALTER TABLE [dbo].[SampleStats]  WITH CHECK ADD  CONSTRAINT [FK_Stats_Stock] FOREIGN KEY([StockID])
REFERENCES [dbo].[Stock] ([ID])
GO
ALTER TABLE [dbo].[SampleStats] CHECK CONSTRAINT [FK_Stats_Stock]
GO
ALTER TABLE [dbo].[Stock]  WITH CHECK ADD  CONSTRAINT [FK_Stock_Sector] FOREIGN KEY([SectorID])
REFERENCES [dbo].[Sector] ([ID])
GO
ALTER TABLE [dbo].[Stock] CHECK CONSTRAINT [FK_Stock_Sector]
GO
ALTER TABLE [dbo].[Stock]  WITH CHECK ADD  CONSTRAINT [CHK_PercentData_LastDay] CHECK  (([PercentData_LastDay]>=(0.00) AND [PercentData_LastDay]<=(100.00)))
GO
ALTER TABLE [dbo].[Stock] CHECK CONSTRAINT [CHK_PercentData_LastDay]
GO
ALTER TABLE [dbo].[Stock]  WITH CHECK ADD  CONSTRAINT [CHK_PercentData_SamplePeriod] CHECK  (([PercentData_SamplePeriod]>=(0.00) AND [PercentData_SamplePeriod]<=(100.00)))
GO
ALTER TABLE [dbo].[Stock] CHECK CONSTRAINT [CHK_PercentData_SamplePeriod]
GO
/****** Object:  StoredProcedure [dbo].[GetDateRange_Period]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetDateRange_Period] @period int = NULL, @stockid bigint
AS
IF(@period is not NULL)
BEGIN
	select distinct top(@period) cast(Date as date) [date]
	 from MinuteData
	 where StockID = @stockid
	 group by [date]
	 order by [date] desc
END
ELSE 
BEGIN
 select distinct cast(Date as date) [date]
 from MinuteData
 where StockID = @stockid
 group by [date]
 order by [date] desc
END
GO
/****** Object:  StoredProcedure [dbo].[GetDateRange_StartEnd]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetDateRange_StartEnd] @start_date datetime = NULL, @end_date datetime = NULL, @stockid bigint
AS
IF(@start_date is not NULL and @end_date is not NULL)
BEGIN
	select distinct cast(Date as date) [date]
	 from MinuteData
	 where StockID = @stockid and [date] between @start_date and DATEADD(DD, 1, @end_date)
	 group by [date]
	 order by [date] desc
END
ELSE 
BEGIN
 select distinct cast(Date as date) [date]
 from MinuteData
 where StockID = @stockid
 group by [date]
 order by [date] desc
END
GO
/****** Object:  StoredProcedure [dbo].[GetHistData]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetHistData] 
	-- Add the parameters for the stored procedure here
	@stockID bigint = NULL
AS
IF(@stockID is not NULL)
BEGIN
select ID, StockID, [Date], ROUND([Close], 2) AS 'Price'
from HistoricalData
WHERE StockID = @stockID
END

ELSE
BEGIN
select ID, StockID, [Date], ROUND([Close], 2) AS 'Price'
from HistoricalData
END
GO
/****** Object:  StoredProcedure [dbo].[GetHistDataJSON]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetHistDataJSON] 
	-- Add the parameters for the stored procedure here
	@stockID bigint = NULL
AS
IF(@stockID is not NULL)
BEGIN
select ID, StockID, [Date], CONVERT(DECIMAL(10,2), ROUND([Close], 2)) AS 'Price'
from HistoricalData
WHERE StockID = @stockID
FOR JSON PATH
END

ELSE
BEGIN
select ID, StockID, [Date], CONVERT(DECIMAL(10,2), ROUND([Close], 2)) AS 'Price'
from HistoricalData
FOR JSON PATH
END

GO
/****** Object:  StoredProcedure [dbo].[GetValuesGroupedBySector_sp]    Script Date: 7/21/2022 11:52:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetValuesGroupedBySector_sp] @StartDate datetime, @EndDate datetime
AS
  SELECT SUM(Value) as Value,
      [Sector]
      ,[SectorId]
  FROM [dbo].[SectorData_V]
  WHERE Time >= @StartDate AND Time <= @EndDate
  GROUP BY [SectorId],[Sector]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "s"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "md"
            Begin Extent = 
               Top = 7
               Left = 290
               Bottom = 170
               Right = 484
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1176
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1356
         SortOrder = 1416
         GroupBy = 1350
         Filter = 1356
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LastUpdatedMinData_V'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LastUpdatedMinData_V'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "s"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 357
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "md"
            Begin Extent = 
               Top = 175
               Left = 48
               Bottom = 338
               Right = 356
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1176
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1356
         SortOrder = 1416
         GroupBy = 1350
         Filter = 1356
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LastUpdatedMinData_VTester'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LastUpdatedMinData_VTester'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "sd"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "s"
            Begin Extent = 
               Top = 7
               Left = 290
               Bottom = 148
               Right = 508
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'SectorData_V'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'SectorData_V'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[18] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "s"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 424
               Right = 339
            End
            DisplayFlags = 280
            TopColumn = 2
         End
         Begin Table = "h"
            Begin Extent = 
               Top = 7
               Left = 387
               Bottom = 170
               Right = 581
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "m"
            Begin Extent = 
               Top = 15
               Left = 696
               Bottom = 324
               Right = 1040
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "se"
            Begin Extent = 
               Top = 15
               Left = 1136
               Bottom = 238
               Right = 1480
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "sst"
            Begin Extent = 
               Top = 51
               Left = 1567
               Bottom = 360
               Right = 1952
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 2616
         Table = 1176
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1356
         SortOrder = 1416
         GroupBy = 1356
         Filter = 1356
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
E' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Stocks_V'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'nd
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Stocks_V'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Stocks_V'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "d"
            Begin Extent = 
               Top = 7
               Left = 306
               Bottom = 170
               Right = 516
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "s"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 258
            End
            DisplayFlags = 280
            TopColumn = 10
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 2700
         Table = 1176
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1356
         SortOrder = 1416
         GroupBy = 1350
         Filter = 1356
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Stocks_V_Old'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Stocks_V_Old'
GO
USE [master]
GO
ALTER DATABASE [NeeksDB] SET  READ_WRITE 
GO
