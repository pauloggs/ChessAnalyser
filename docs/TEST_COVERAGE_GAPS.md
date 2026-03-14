# Test Coverage Gaps

This document identifies test coverage in the ChessAnalyser codebase. It was originally written when many components had no or empty tests; since then, most critical and supporting gaps have been addressed.

---

## 1. Now Covered (Previously Critical)

### 1.1 **FileHandler** / **FileHandlerTests**
- **Location:** `src/Services/FileHandler.cs`, `test/ServicesTests/FileHandlerTests.cs`
- **Status:** ✅ **Covered.** Tests for single file, directory with PGNs, non-existent path, invalid path.

### 1.2 **PgnParser** / **PgnParserTests**
- **Location:** `src/Services/PgnParser.cs`, `test/ServicesTests/PgnParserTests.cs`
- **Status:** ✅ **Covered.** Tests for single/multiple games, source file name and game index, empty list, `SetBoardPositions` delegation.

### 1.3 **BoardPositionService**
- **Location:** `src/Services/BoardPositionService.cs`, `test/ServicesTests/BoardPositionServiceTests.cs`
- **Status:** ✅ **Covered.** Tests for single game flow and early exit when `SetWinner` returns true.

### 1.4 **BoardPositionCalculator** and **BoardPositionCalculatorHelper**
- **Location:** `src/Services/Helpers/BoardPositionCalculator.cs`, `BoardPositionCalculatorHelper.cs`, `test/ServicesTests/BoardPositionCalculatorTests.cs`, `BoardPositionCalculatorHelperTests.cs`
- **Status:** ✅ **Covered.** Tests for routing (en-passant, promotion, non-promotion, castling, unrecognized), non-promotion pawn move, double push en-passant target, piece at destination throws, kingside/queenside castling, **en-passant** (valid capture and no-target throws), **promotion** (white to queen, square occupied throws).

### 1.5 **BoardPositionsHelper.GetBoardPositionForPly**
- **Location:** `test/ServicesHelpersTests/BoardPositionsHelperTests.cs`
- **Status:** ✅ **Covered.** Tests for using initial/previous position, updating ply, calling calculator, null game/initial position/missing previous position, full starting bitboard layout.

### 1.6 **DestinationSquareHelper**
- **Location:** `test/ServicesHelpersTests/DestinationSquareHelperTests.cs`
- **Status:** ✅ **Covered.** Tests for Nf3, e4, e8=Q, castling (-1), Bxe5, **neither piece move nor castling throws**, **malformed short move throws**.

### 1.7 **PersistenceService**
- **Location:** `test/ServicesTests/PersistenceServiceTests.cs`
- **Status:** ✅ **Covered.** Tests for `GetUnprocessedGames` (filtering) and `InsertGames` (repository calls).

### 1.8 **EtlService**
- **Location:** `test/ServicesTests/EtlServiceTests.cs`
- **Status:** ✅ **Covered.** Tests for full load flow and no unprocessed games.

### 1.9 **ChessRepository** / **ChessRepositoryTests**
- **Location:** `test/RepositoriesTests/ChessRepositoryTests.cs`
- **Status:** ✅ **Covered.** Constructor and `GetProcessedGameIds` when connection fails.

### 1.10 **AnalyserController** / **AnalyserControllerTests**
- **Location:** `test/AnalyserTests/AnalyserControllerTests.cs`
- **Status:** ✅ **Covered.** Load games (OK and 500 on ETL throw), GetGames.

---

## 2. Partially Tested (Improvements Done)

### 2.1 **MoveInterpreter**
- **Location:** `test/ServicesTests/MoveInterpreterWithRealHelperTests.cs`, `MoveInterpreterTests.cs`
- **Status:** ✅ **Covered with real helper.** e4, Nf3, e4+ (check stripped) with full pipeline.

### 2.2 **PieceSourceFinderService** (bishop, rook, queen)
- **Location:** `test/ServicesHelpersTests/PieceSourceFinderServiceTests.cs`
- **Status:** ✅ **Covered.** Knight, king, bishop, rook, queen and “no knight can reach” tests with real dependencies.

### 2.3 **BoardPositionsHelper.GetStartingBoardPosition**
- **Location:** `test/ServicesHelpersTests/BoardPositionsHelperTests.cs`
- **Status:** ✅ **Covered.** Full bitboard layout asserted for starting position.

---

## 3. Supporting / Lower-Risk (Now Covered)

### 3.1 **Naming**
- **Location:** `test/ServicesTests/NamingTests.cs`
- **Status:** ✅ **Covered.** All tags present, spaces to hyphens, missing key throws.

### 3.2 **DisplayService**
- **Location:** `test/ServicesTests/DisplayServiceTests.cs`
- **Status:** ✅ **Covered.** Single piece, black piece, empty board for `GetBoardArrayFromBoardPositions`.

### 3.3 **DirectoryProcessor** / **FileHandlerHelper**
- **Location:** `test/ServicesHelpersTests/DirectoryProcessorTests.cs`, `FileHandlerHelperTests.cs`
- **Status:** ✅ **Covered.** Load single file, append to list, file not found; process directory (one file, recursive subdir, empty).

### 3.4 **StringExtensions**
- **Location:** `test/ServicesHelpersTests/StringExtensionsTests.cs`
- **Status:** ✅ **Covered.** `RemoveLineEndings` for `\n`, `\r\n`, mixed, empty, no line endings.

### 3.5 **ExtensionMethods** (Interfaces)
- **Location:** `test/ServicesHelpersTests/ExtensionMethodsTests.cs`
- **Status:** ✅ **Covered.** `DeepCopy` (independent copy, null throws), `GetSquareFromRankAndFile`, `Algebraic`.

---

## 4. Integration Tests

### 4.1 **PGN pipeline (real PGN files)**
- **Location:** `test/ServicesTests/PgnPipelineIntegrationTests.cs`
- **Status:** ✅ **Covered.** Full pipeline (FileHandler → PgnParser → BoardPositionService) for:
  - `checkmate.pgn`
  - `king-side-castling-with-numerics.pgn`
  - `promotion.pgn`
  - `queen-side-castling-with-numerics.pgn`
  - `en-passant.pgn` (minimal game with en-passant capture)

---

## 5. Summary Table (Current State)

| Component | Test file | Coverage |
|-----------|-----------|----------|
| FileHandler | FileHandlerTests | ✅ |
| PgnParser | PgnParserTests | ✅ |
| BoardPositionService | BoardPositionServiceTests | ✅ |
| BoardPositionCalculator | BoardPositionCalculatorTests | ✅ |
| BoardPositionCalculatorHelper | BoardPositionCalculatorHelperTests | ✅ (incl. en-passant, promotion) |
| BoardPositionsHelper | BoardPositionsHelperTests | ✅ |
| DestinationSquareHelper | DestinationSquareHelperTests | ✅ (incl. invalid/malformed) |
| PersistenceService | PersistenceServiceTests | ✅ |
| EtlService | EtlServiceTests | ✅ |
| ChessRepository | ChessRepositoryTests | ✅ |
| AnalyserController | AnalyserControllerTests | ✅ |
| MoveInterpreter (real helper) | MoveInterpreterWithRealHelperTests | ✅ |
| PieceSourceFinder (N/K/B/R/Q) | PieceSourceFinderServiceTests | ✅ |
| GetStartingBoardPosition | BoardPositionsHelperTests | ✅ (full layout) |
| Naming | NamingTests | ✅ |
| DisplayService | DisplayServiceTests | ✅ |
| DirectoryProcessor / FileHandlerHelper | DirectoryProcessorTests, FileHandlerHelperTests | ✅ |
| StringExtensions | StringExtensionsTests | ✅ |
| ExtensionMethods | ExtensionMethodsTests | ✅ |
| PGN pipeline integration | PgnPipelineIntegrationTests | ✅ (5 PGNs) |

---

## 6. Remaining / Optional Gaps

- **BitBoardManipulator**  
  - `MovePiece(BoardPosition, Ply)` validation and bitboard logic are tested (e.g. invalid squares, piece moves). `ReadSquare(BoardPosition, int square)` (returns `(Piece?, Colour?)`) is covered for empty and occupied squares. Deeper bitboard edge cases (e.g. multiple pieces, every piece type) are optional.

- **SquareHelper**  
  - Out-of-range and valid inputs are tested in `SquareHelperTests`. No further gaps identified.

- **ChessRepository**  
  - Only constructor and failure path are tested. `InsertGame` / `InsertBoardPositions` with a real or in-memory database would require test infrastructure (e.g. local DB or testcontainers). Mark as optional for future work.

- **DisplayService.DisplayBoardPosition**  
  - Only `GetBoardArrayFromBoardPositions` is tested. Console output is not asserted; acceptable for a display helper.

- **En-passant detection in PGN**  
  - Covered via `en-passant.pgn` integration test and `BoardPositionCalculatorHelper` en-passant tests. No separate “parser detects e.p. suffix” test; optional.

---

## 7. Recommended Next Steps (If Extending Further)

1. Add repository tests with a test database or in-memory provider if persistence behaviour becomes critical.
2. Add more PGN edge-case files (e.g. resignation, time forfeit, unusual annotations) if parsing robustness is a priority.
3. Add targeted tests for **BitBoardManipulator** (e.g. `ReadSquare`, `AddPiece`/`RemovePiece` with various piece sets) if bitboard logic is changed often.

Using the current test set will catch regressions in parsing, board position calculation, move interpretation, source finding, and the full PGN pipeline.
