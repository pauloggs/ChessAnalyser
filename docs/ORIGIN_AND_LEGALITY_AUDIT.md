# Origin square and legality – audit (all piece types)

## Summary (after implementation)

| Piece | (1) Origin correct | (2) Only legal moves | (3) Efficient | (4) Blocking / disambiguation |
|-------|--------------------|----------------------|---------------|--------------------------------|
| **Knight** | Yes | Yes (pin filter when multiple) | Legal check only when candidates > 1 | Multiple: legality + tie-break |
| **Bishop** | Yes | Yes | Legal filter only when we have candidates | Blocking: ray from dest; multiple: collect all → legal → tie-break |
| **Rook** | Yes | Yes | Same | Same |
| **Queen** | Yes (rook + bishop candidates) | Yes | Same | Same |
| **King** | Yes | Yes (reject if moves into check) | Single candidate, one legal check | N/A |
| **Pawn** | Yes | Yes (final check before apply) | One check at apply time | Push: 1 then 2 back; capture: file from PGN |

## Details

### (1) Origin square

- **Knights:** All candidates along the 8 L-shapes are collected; when multiple, filtered by legality (pin), then tie-break. Correct.
- **Bishops / Rooks / Queens:** Ray-cast from the destination. The **first** piece of the right type on each ray can reach the destination (blocking is correct). But when **several** pieces can reach the same square (e.g. two rooks on the e-file), the code **returns the first match** by direction order and does **not** collect the others, so it can pick the wrong one. Also no pin check.
- **King:** Only one king; source is correct. No check that the king doesn’t move into check.
- **Pawn:** Non-capture: one or two squares back; capture: file from PGN. At most one candidate per move; blocking handled. No pin check.

### (2) Only legal moves

- **Knights:** Legality (would move leave king in check) is applied when there are multiple candidates. Single candidate is not re-checked (correct, no need).
- **Bishops, Rooks, Queens, King, Pawn:** No legality check. A pinned piece or king moving into check can be chosen.

### (3) Efficiency

- **Knights:** Legal move check is only used when `candidates.Count > 1`. Single candidate returns without calling the checker. Efficient.
- **Others:** No legal check, so no extra cost (but also not correct).

### (4) Blocking and PGN disambiguation

- **Sliding pieces (B, R, Q):** Blocking is handled efficiently: rays are cast from the **destination**; the first piece hit on each ray is the one that can reach the square (path is clear). No need to test “intervening” pieces explicitly.
- **Knights:** No blocking. When PGN does not disambiguate, we use legality (and tie-break) to choose among multiple knights.
- **Pawns:** Single candidate per move; blocking implied by “one or two squares back” and capture file.
- **Bishops/Rooks/Queens when PGN doesn’t disambiguate:** We do **not** collect all candidates and then choose the only legal one; we return the first by iteration order, so we can choose a pinned or wrong piece.

## Implemented behaviour

1. **Bishop / Rook / Queen:** Collect all candidates (all rays that have our piece as first piece); then filter by `!WouldMoveLeaveKingInCheck`; then if one candidate return it, if none return MoveNotFound, if several apply a deterministic tie-break (e.g. smallest square index). Only call the legal checker when there are candidates (and only when needed for filtering).
2. **King:** After finding the (unique) king source, check `WouldMoveLeaveKingInCheck`. If true, return MoveNotFound (or throw) so we never move the king into check.
3. **Pawn:** No multiple-candidate list; add a single legality check before applying the move (in the same path as other pieces) so that a pinned pawn is rejected.
4. **Single place for “only legal moves”:** Before applying any piece move (including pawn and king), call `WouldMoveLeaveKingInCheck` once; if true, throw. This guarantees only legal moves are applied and covers pawn/king and any remaining edge cases.
