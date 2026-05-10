/****** Object:  Table [dbo].[GameMove]    Script Date: 10/05/2026 16:17:21 ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GameMove](
	[GameId] [int] NOT NULL,
	[PlyIndex] [int] NOT NULL,
	[MovingSide] [char](1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[FromSquare] [tinyint] NOT NULL,
	[ToSquare] [tinyint] NOT NULL,
	[MovedPiece] [char](1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CapturedPiece] [char](1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PromotionPiece] [char](1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IsCastlingKingside] [bit] NOT NULL,
	[IsCastlingQueenside] [bit] NOT NULL,
 CONSTRAINT [PK_GameMove] PRIMARY KEY CLUSTERED 
(
	[GameId] ASC,
	[PlyIndex] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING ON

/****** Object:  Index [IX_GameMove_ToSquare_MovedPiece]    Script Date: 10/05/2026 16:17:21 ******/
CREATE NONCLUSTERED INDEX [IX_GameMove_ToSquare_MovedPiece] ON [dbo].[GameMove]
(
	[ToSquare] ASC,
	[MovedPiece] ASC
)
INCLUDE([GameId],[PlyIndex],[MovingSide]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[GameMove] ADD  CONSTRAINT [DF_GameMove_IsCastlingKingside]  DEFAULT ((0)) FOR [IsCastlingKingside]
ALTER TABLE [dbo].[GameMove] ADD  CONSTRAINT [DF_GameMove_IsCastlingQueenside]  DEFAULT ((0)) FOR [IsCastlingQueenside]
ALTER TABLE [dbo].[GameMove]  WITH CHECK ADD  CONSTRAINT [FK_GameMove_Game] FOREIGN KEY([GameId])
REFERENCES [dbo].[Game] ([Id])
ALTER TABLE [dbo].[GameMove] CHECK CONSTRAINT [FK_GameMove_Game]
ALTER TABLE [dbo].[GameMove]  WITH CHECK ADD  CONSTRAINT [CK_GameMove_CapturedPiece] CHECK  (([CapturedPiece] IS NULL OR ([CapturedPiece]='K' OR [CapturedPiece]='Q' OR [CapturedPiece]='R' OR [CapturedPiece]='B' OR [CapturedPiece]='N' OR [CapturedPiece]='P')))
ALTER TABLE [dbo].[GameMove] CHECK CONSTRAINT [CK_GameMove_CapturedPiece]
ALTER TABLE [dbo].[GameMove]  WITH CHECK ADD  CONSTRAINT [CK_GameMove_FromSquare] CHECK  (([FromSquare]>=(0) AND [FromSquare]<=(63)))
ALTER TABLE [dbo].[GameMove] CHECK CONSTRAINT [CK_GameMove_FromSquare]
ALTER TABLE [dbo].[GameMove]  WITH CHECK ADD  CONSTRAINT [CK_GameMove_MovedPiece] CHECK  (([MovedPiece]='K' OR [MovedPiece]='Q' OR [MovedPiece]='R' OR [MovedPiece]='B' OR [MovedPiece]='N' OR [MovedPiece]='P'))
ALTER TABLE [dbo].[GameMove] CHECK CONSTRAINT [CK_GameMove_MovedPiece]
ALTER TABLE [dbo].[GameMove]  WITH CHECK ADD  CONSTRAINT [CK_GameMove_MovingSide] CHECK  (([MovingSide]='B' OR [MovingSide]='W'))
ALTER TABLE [dbo].[GameMove] CHECK CONSTRAINT [CK_GameMove_MovingSide]
ALTER TABLE [dbo].[GameMove]  WITH CHECK ADD  CONSTRAINT [CK_GameMove_PromotionPiece] CHECK  (([PromotionPiece] IS NULL OR ([PromotionPiece]='Q' OR [PromotionPiece]='R' OR [PromotionPiece]='B' OR [PromotionPiece]='N')))
ALTER TABLE [dbo].[GameMove] CHECK CONSTRAINT [CK_GameMove_PromotionPiece]
ALTER TABLE [dbo].[GameMove]  WITH CHECK ADD  CONSTRAINT [CK_GameMove_ToSquare] CHECK  (([ToSquare]>=(0) AND [ToSquare]<=(63)))
ALTER TABLE [dbo].[GameMove] CHECK CONSTRAINT [CK_GameMove_ToSquare]

