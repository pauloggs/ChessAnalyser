/****** Object:  Table [dbo].[BoardPosition]    Script Date: 10/05/2026 12:09:38 ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[BoardPosition](
	[GameId] [int] NOT NULL,
	[PlyIndex] [int] NOT NULL,
	[WP] [bigint] NOT NULL,
	[WN] [bigint] NOT NULL,
	[WB] [bigint] NOT NULL,
	[WR] [bigint] NOT NULL,
	[WQ] [bigint] NOT NULL,
	[WK] [bigint] NOT NULL,
	[BP] [bigint] NOT NULL,
	[BN] [bigint] NOT NULL,
	[BB] [bigint] NOT NULL,
	[BR] [bigint] NOT NULL,
	[BQ] [bigint] NOT NULL,
	[BK] [bigint] NOT NULL,
	[EnPassantTargetFile] [char](1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_BoardPosition] PRIMARY KEY CLUSTERED 
(
	[GameId] ASC,
	[PlyIndex] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

/****** Object:  Index [IX_BoardPosition_GameId]    Script Date: 10/05/2026 12:09:38 ******/
CREATE NONCLUSTERED INDEX [IX_BoardPosition_GameId] ON [dbo].[BoardPosition]
(
	[GameId] ASC
)
INCLUDE([PlyIndex]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [WP]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [WN]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [WB]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [WR]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [WQ]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [WK]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [BP]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [BN]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [BB]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [BR]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [BQ]
ALTER TABLE [dbo].[BoardPosition] ADD  DEFAULT ((0)) FOR [BK]
ALTER TABLE [dbo].[BoardPosition]  WITH CHECK ADD  CONSTRAINT [FK_BoardPosition_Game] FOREIGN KEY([GameId])
REFERENCES [dbo].[Game] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[BoardPosition] CHECK CONSTRAINT [FK_BoardPosition_Game]

