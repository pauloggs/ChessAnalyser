SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GamePositionSummary](
	[GameId] [int] NOT NULL,
	[PlyIndex] [int] NOT NULL,
	[WhiteMaterial] [smallint] NOT NULL,
	[BlackMaterial] [smallint] NOT NULL,
	[WhitePawnCount] [smallint] NOT NULL,
	[WhiteKnightCount] [smallint] NOT NULL,
	[WhiteBishopCount] [smallint] NOT NULL,
	[WhiteRookCount] [smallint] NOT NULL,
	[WhiteQueenCount] [smallint] NOT NULL,
	[WhiteKingCount] [smallint] NOT NULL,
	[BlackPawnCount] [smallint] NOT NULL,
	[BlackKnightCount] [smallint] NOT NULL,
	[BlackBishopCount] [smallint] NOT NULL,
	[BlackRookCount] [smallint] NOT NULL,
	[BlackQueenCount] [smallint] NOT NULL,
	[BlackKingCount] [smallint] NOT NULL,
 CONSTRAINT [PK_GamePositionSummary] PRIMARY KEY CLUSTERED 
(
	[GameId] ASC,
	[PlyIndex] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_GamePositionSummary_PlyIndex] ON [dbo].[GamePositionSummary]
(
	[PlyIndex] ASC
)
INCLUDE([WhiteMaterial],[BlackMaterial],[GameId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[GamePositionSummary]  WITH CHECK ADD  CONSTRAINT [FK_GamePositionSummary_Game] FOREIGN KEY([GameId])
REFERENCES [dbo].[Game] ([Id])
ALTER TABLE [dbo].[GamePositionSummary] CHECK CONSTRAINT [FK_GamePositionSummary_Game]
