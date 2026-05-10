/****** Object:  Table [dbo].[GameParseError]    Script Date: 10/05/2026 16:17:21 ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GameParseError](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SourcePgnFileName] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[GameIndexInFile] [int] NULL,
	[GameName] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ErrorMessage] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[RecordedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_GameParseError] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[GameParseError] ADD  DEFAULT (sysutcdatetime()) FOR [RecordedAt]

