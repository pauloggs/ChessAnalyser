# Test Coverage Gaps

This document identifies test coverage gaps in the ChessAnalyser codebase (focusing on code that existed before recent AI-assisted improvements). Addressing these gaps would have caught bugs such as the knight/king source-finder sign error and invalid square handling in `MovePiece`.

---

## 1. Critical: No Tests (or Empty Tests)

### 1.1 **FileHandler** / **FileHandlerTests**
- **Location:** `src/Services/FileHandler.cs`, `test/ServicesTests/FileHandlerTests.cs`
- **Gap:** `FileHandlerTests` contains only an empty `Test1()` with no assertions. `FileHandler.LoadPgnFiles(string path)` is **never tested** (file vs directory, missing path, `FileHandlerHelper`/`DirectoryProcessor` integration).
- **Risk:** Regressions in file/directory loading and path handling go undetected.

### 1.2 **PgnParser** / **PgnParserTests**
- **Location:** `src/Services/PgnParser.cs`, `test/ServicesTests/PgnParserTests.cs`
- **Gap:** The method `GetPgnGamesFromPgnFiles_ShouldReturnGames()` has **no `[Fact]` attribute** (so it is not run), and its body is empty. No test verifies that `GetGamesFromPgnFiles` returns games from a list of `PgnFile`, assigns `SourcePgnFileName`/`GameIndexInFile`, or that `GameIdGenerator.GetGameId` is applied.
- **Risk:** Regressions in the main parsing entry point and game-ID assignment are untested.

### 1.3 **BoardPositionService**
- **Location:** `src/Services/BoardPositionService.cs`
- **Gap:** **No dedicated test class.** It is only mocked in `PgnParserTests`. Its behaviour (iterating games, calling `GetStartingBoardPosition`, `GetBoardPositionForPly`, `SetWinner`, populating `game.BoardPositions`) is never asserted.
- **Risk:** Orchestration bugs (e.g. wrong ply order, wrong game/board association) are untested.

### 1.4 **BoardPositionCalculator** and **BoardPositionCalculatorHelper**
- **Location:** `src/Services/Helpers/BoardPositionCalculator.cs`, `BoardPositionCalculatorHelper.cs`
- **Gap:** **No tests at all.** The entire “apply one ply to a board” pipeline is untested:
  - `GetBoardPositionFromPly` (routing to en-passant, promotion, non-promotion, castling)
  - `GetBoardPositionFromEnPassant`
  - `GetBoardPositionFromPromotion`
  - `GetBoardPositionFromNonPromotion`
  - `GetBoardPositionFromKingSideCastling` / `GetBoardPositionFromQueenSideCastling`
- **Risk:** This is the core of move application. Bugs here (e.g. wrong bitboard updates, missing validation) would not be caught by unit tests. The knight source-finder bug only surfaced when this path was exercised end-to-end.

### 1.5 **BoardPositionsHelper.GetBoardPositionForPly**
- **Location:** `src/Services/Helpers/BoardPositionsHelper.cs`
- **Gap:** `BoardPositionsHelperTests` covers `GetStartingBoardPosition` and `SetWinner` only. **`GetBoardPositionForPly` is never tested** (obtaining previous position, calling `MoveInterpreter.GetSourceAndDestinationSquares`, updating the ply, calling `BoardPositionCalculator.GetBoardPositionFromPly`, `BuildParsingContext`).
- **Risk:** Integration between move interpretation and board calculation is unverified; context for error messages could be wrong.

### 1.6 **DestinationSquareHelper**
- **Location:** `src/Services/Helpers/DestinationSquareHelper.cs`
- **Gap:** **No direct tests.** It is only mocked in `MoveInterpreterHelperTests`. Logic for piece moves (including promotion, e.g. `e8=Q`), castling (-1), and invalid/malformed moves is untested.
- **Risk:** Wrong destination square (e.g. off-by-one in rank/file or promotion parsing) would propagate to source-finding and board updates.

### 1.7 **PersistenceService**
- **Location:** `src/Services/PersistenceService.cs`
- **Gap:** **No tests.** `GetUnprocessedGames` and `InsertGames` (and thus interaction with `IChessRepository`) are untested.
- **Risk:** Filtering of already-processed games and insert/board-position persistence could be wrong without any test failing.

### 1.8 **EtlService**
- **Location:** `src/Services/EtlService.cs`
- **Gap:** **No tests.** The full ETL flow (`LoadPgnFiles` → `GetGamesFromPgnFiles` → `GetUnprocessedGames` → `SetBoardPositions` → `InsertGames`) is untested.
- **Risk:** Orchestration and error handling across file handling, parsing, persistence, and board generation are unverified.

### 1.9 **ChessRepository** / **ChessRepositoryTests**
- **Location:** `src/Repositories/ChessRepository.cs`, `test/RepositoriesTests/ChessRepositoryTests.cs`
- **Gap:** `ChessRepositoryTests` contains only an empty `Test1()`. No tests for `GetProcessedGameIds`, `InsertGame`, `InsertBoardPositions`, or SQL/parameter usage.
- **Risk:** Data access and SQL correctness are unverified (unless covered by integration tests elsewhere).

### 1.10 **AnalyserController** / **AnalyserControllerTests**
- **Location:** `src/Analyser/Controllers/Analyser.cs`, `test/AnalyserTests/AnalyserControllerTests.cs`
- **Gap:** `AnalyserControllerTests` contains only an empty `Test1()`. No tests for the API endpoint(s) or interaction with `IEtlService`.
- **Risk:** Controller behaviour and HTTP contract are untested.

---

## 2. Partially Tested or Mock-Heavy

### 2.1 **MoveInterpreter**
- **Location:** `src/Services/MoveInterpreter.cs`, `test/ServicesTests/MoveInterpreterTests.cs`
- **Gap:** Tests use a **mocked** `IMoveInterpreterHelper` only (null ply, invalid raw move, and that the helper is called with expected return values). There are **no tests with the real `MoveInterpreterHelper`** (and thus real `GetPiece`, `GetDestinationSquare`, `GetSourceSquare` via pawn/piece interpreters). The real pipeline from raw move to (piece, sourceSquare, destinationSquare) is untested.
- **Risk:** Bugs in the real helper chain (e.g. destination square, piece type, or source square) would not be caught.

### 2.2 **PieceSourceFinderService** (bishop, rook, queen)
- **Location:** `src/Services/Helpers/PieceSourceFinderService.cs`
- **Gap:** **Knight and king** are now covered by `PieceSourceFinderServiceTests` (with real board and real dependencies). **Bishop, rook, and queen** (`FindBishopSource`, `FindRookSource`, `FindQueenSource`) have no analogous tests with real board positions.
- **Risk:** Similar “direction” or indexing bugs in sliding-piece source finding could go undetected.

### 2.3 **BoardPositionsHelper.GetStartingBoardPosition**
- **Location:** `test/ServicesHelpersTests/BoardPositionsHelperTests.cs`
- **Gap:** Only asserts “not null” and “PiecePositions not null”. **No assertion that the 12 piece bitboards match the standard starting position** (e.g. WP on rank 2, WN on b1/g1, etc.).
- **Risk:** Wrong initial bitboard layout would not be detected.

---

## 3. Supporting / Lower-Risk Gaps

### 3.1 **Naming**
- **Location:** `src/Services/Naming.cs`
- **Gap:** No tests for `GetGameName` (format, missing/malformed tags, key not present).
- **Risk:** Low for core correctness; higher for display/naming consistency.

### 3.2 **DisplayService**
- **Location:** `src/Services/DisplayService.cs`
- **Gap:** No tests for `GetBoardArrayFromBoardPositions` or display output. Logic (e.g. `col = key[0] == 'W' ? 1 : 1`) is subtle and could hide display bugs.
- **Risk:** Display-only; no impact on persistence or analysis if unused there.

### 3.3 **DirectoryProcessor** / **FileHandlerHelper**
- **Location:** `src/Services/Helpers/DirectoryProcessor.cs`, `FileHandlerHelper.cs`
- **Gap:** No direct tests. Only exercised via `FileHandler`, which is itself untested.
- **Risk:** Recursion, file filtering, or path handling could be wrong.

### 3.4 **StringExtensions**
- **Location:** `src/Services/Helpers/StringExtensions.cs`
- **Gap:** No tests for `RemoveLineEndings` (e.g. `\n`, `\r\n`, mixed).
- **Risk:** Low; single regex, but used in PGN parsing.

### 3.5 **ExtensionMethods** (Interfaces)
- **Location:** `src/Interfaces/ExtensionMethods.cs`
- **Gap:** No tests for `DeepCopy` (BoardPosition), `GetSquareFromRankAndFile`, or `Algebraic(int)`.
- **Risk:** `DeepCopy` is used in board updates; wrong copy would be serious. The other two are simple but used widely.

---

## 4. Summary Table

| Component | Test file | Coverage |
|-----------|-----------|----------|
| FileHandler | FileHandlerTests | ❌ Empty |
| PgnParser | PgnParserTests | ❌ No [Fact], empty test |
| BoardPositionService | — | ❌ None |
| BoardPositionCalculator | — | ❌ None |
| BoardPositionCalculatorHelper | — | ❌ None |
| BoardPositionsHelper.GetBoardPositionForPly | BoardPositionsHelperTests | ❌ Not tested |
| DestinationSquareHelper | — | ❌ None (only mocked) |
| PersistenceService | — | ❌ None |
| EtlService | — | ❌ None |
| ChessRepository | ChessRepositoryTests | ❌ Empty |
| AnalyserController | AnalyserControllerTests | ❌ Empty |
| MoveInterpreter (real helper) | MoveInterpreterTests | ⚠️ Mocked only |
| PieceSourceFinder (B/R/Q) | PieceSourceFinderServiceTests | ⚠️ Knight/King only |
| GetStartingBoardPosition | BoardPositionsHelperTests | ⚠️ Shallow (not null only) |
| Naming, DisplayService, DirectoryProcessor, FileHandlerHelper, StringExtensions, ExtensionMethods | — | ⚠️ None / low risk |

---

## 5. Recommended Priorities

1. **High:** Add tests for **BoardPositionCalculator** / **BoardPositionCalculatorHelper** (at least one test per move type: normal piece, capture, promotion, en-passant, kingside/queenside castling) with real `BoardPosition` and `Ply`. This is the core that would have caught the knight/king source bug earlier if covered.
2. **High:** Add tests for **BoardPositionsHelper.GetBoardPositionForPly** (e.g. one simple game with a few plies, real dependencies or a test double for the calculator that records inputs).
3. **High:** Implement **PgnParserTests.GetPgnGamesFromPgnFiles** with `[Fact]` and real or in-memory PGN content; assert game count, `SourcePgnFileName`, `GameIndexInFile`, and that `GameId` is set.
4. **Medium:** Add **DestinationSquareHelper** tests (piece move, promotion, castling, invalid move).
5. **Medium:** Add **FileHandler** tests (file path, directory path, non-existent path) and fill **ChessRepositoryTests** (e.g. with an in-memory or test double repository).
6. **Medium:** Add **PieceSourceFinderService** tests for **bishop, rook, queen** (real board, correct source square).
7. **Lower:** **PersistenceService**, **EtlService**, **BoardPositionService** (with mocks or in-memory repo); **Naming**; **ExtensionMethods.DeepCopy**; **AnalyserController** (HTTP and ETL wiring).

Using this list to add tests will significantly reduce the risk of regressions and make it much easier to catch bugs similar to the knight/king source-finder and invalid-square issues before they reach production.
