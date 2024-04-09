# FileUploadCore8


Please follow below steps to run the application

1) create a database/ existing database and run below script to create file details table
/****** Object:  Table [dbo].[FileDetails]    Script Date: 4/9/2024 7:17:37 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FileDetails](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](80) NOT NULL,
	[FileData] [varbinary](max) NOT NULL,
	[FileType] [nvarchar](50) NULL,
	[ContentType] [nvarchar](50) NULL,
 CONSTRAINT [PK_FileDetails] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


2) In "appsettings.json" file add "DefaultConnection" details for "Data Source' and "Initial Catalog" details for your database 
3) run Manage NuGet packages
4)  Core 8 version is used
