# Game 28 review – "No opposing piece at destination" at Ply 29 (Nxe4)

## (1) Revert

The previous “lenient” handling (treating capture-on-empty as non-capture) has been **reverted**. The parser again throws when there is no opposing piece at the destination for a capture move.

---

## (2) Game 28 identification

- **PGN:** Alekhine.pgn  
- **Game index in file:** 28 (28th game)  
- **Tags:** Event "?", Site "Moscow Club Spring", Date "1908.??.??", White "Shaposhnikov N", Black "Alekhine, Alexander A", Result "0-1", ECO "B20"  
- **Relevant moves (around the error):**
  - 14. Bd2 Nf6  
  - **15. Ne4** (ply 28 – White)  
  - **15... Nxe4** (ply 29 – Black; capture on e4)

So at ply 29 the move is **Black Nxe4**: Black’s knight captures whatever is on e4. After 15. Ne4, that must be the **white knight**. So the position after ply 28 must have a **white knight on e4** (square 28).

---

## (3) Conclusion: (a) parsing or (b) file?

- **File (b):** The move sequence in the PGN is consistent and legal: 15. Ne4 15...Nxe4 is normal (White puts a knight on e4, Black takes it). There is no sign of a typo or wrong move order in the file for this game.
- **Parsing (a):** The error “There is no opposing piece at the destination square 28 for a capture move” means that when we apply ply 29 (Nxe4), `ReadSquare(previousBoardPosition, 28)` finds **no piece** on e4. So the **position we computed after ply 28** does not have a white knight on e4. That implies:
  - Either **15. Ne4 (ply 28)** was applied incorrectly (wrong source/destination, or wrong piece), so the knight never ended up on e4, or
  - The **position after ply 27** was already wrong, so applying 15. Ne4 from that position still never puts a knight on e4.

So the failure is in **our parsing/board-update logic**, not in the PGN file.

**Verdict: (a) parsing** – the file is correct; the bug is in how we compute or apply the board position (most likely for ply 28, 15. Ne4).

---

## (4) Resolution (implemented)

1. **Reproduce:** An integration test was added: `FullPipeline_Game28_AfterPly28_WhiteKnightOnE4` loads Game 28, runs the pipeline, and asserts that the position after ply 28 has a white knight on e4 (square 28).

2. **Root cause:** The position after ply 27 had white knights on **e2 and d4** instead of **c3 and e2**. For **12.Ne2** both knights (c3 and d4) can reach e2; the parser chose the **leftmost** (c3), so it moved the wrong knight and left no knight on c3 for **15.Ne4**.

3. **Fix (parser):**
   - **PieceSourceFinderService.FindKnightSource:** When multiple knights can reach the destination and none is on a home square, **prefer the rightmost knight** (largest file index) instead of the first candidate. That makes 12.Ne2 use the knight from d4, leaving the knight on c3 for 15.Ne4.
   - The same tie-break was applied in the **fallback** path (when the main loop finds 0 candidates but the bitboard has 2+ knights on the 8 candidate squares): collect all, apply home-square preference, then prefer rightmost.
   - **PlyHelper:** Optional robustness: strip full-width period in move-number regex; strip leading move number from a token when the line-level regex missed it; skip tokens that are purely digits. (Skipping game result tokens was reverted so `SetWinner` still sees "1-0"/"0-1" and sets `game.Winner`.)

No change to the PGN file was required.
