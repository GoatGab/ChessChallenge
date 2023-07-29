using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot { 

    int pieceValue(Piece piece)
    {
        return piece.IsKing ? 1000 : piece.IsQueen ? 900 : piece.IsRook ? 500 : piece.IsKnight || piece.IsBishop ? 300 : piece.IsPawn ? 100 : 0;
    }

    int pieceTypeValue(PieceType piece)
    {
        List<PieceType> values = Enum.GetValues(typeof(PieceType))
                    .Cast<PieceType>()
                    .ToList();
        return values.IndexOf(piece) * 100;
    }

    int boolToInt(bool var)
    {
        if (var) return 1;
        return -1;
    }
    Move[] orderMoves(Board board, Move[] moves)
    {
        int[] moveScoreGuesses = new int[moves.Length];
        int i = 0;
        foreach (Move move in moves)
        {
            if (move.CapturePieceType != PieceType.None) moveScoreGuesses[i] = 10 * pieceTypeValue(move.CapturePieceType) - pieceTypeValue(move.MovePieceType);
            if (move.IsPromotion) moveScoreGuesses[i] += pieceTypeValue(move.PromotionPieceType);
            if (board.SquareIsAttackedByOpponent(move.TargetSquare)) moveScoreGuesses[i] -= 5 * pieceTypeValue(move.MovePieceType);
            i++;
        }
        List<Move> sortedMoves = moves.ToList();
        sortedMoves = sortedMoves.OrderBy(o => moveScoreGuesses).ToList();
        return sortedMoves.ToArray();
    }

    int materialEval(Board board)
    {
        PieceList[] pieces = board.GetAllPieceLists();
        int whiteScore = 0;
        int blackScore = 0;
        for(int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].IsWhitePieceList) whiteScore += pieces[i].Count * pieceValue(pieces[i].GetPiece(0));
            else blackScore += pieces[i].Count * pieceValue(pieces[i].GetPiece(0));
        }
        return boolToInt(board.IsWhiteToMove) * (whiteScore - blackScore);

    }
    int captureEval(Board board, Move move)
    {
        int captureValue = 0;
        if (move.IsCapture) captureValue += pieceTypeValue(move.CapturePieceType) - pieceTypeValue(move.MovePieceType);
        return boolToInt(board.IsWhiteToMove) * captureValue;
    }

    float map_value(float n, float start1, float stop1, float start2, float stop2)
    {
        return (n - start1) / (stop1 - start1) * (stop2 - start2) + start2;
    }

    int ForceKingToCornerEndgameEval(Square friendlyKingSquare, Square opponentKingSquare, float endgameWeight)
    {
        int evaluation = 0;
        int opponentKingRank = friendlyKingSquare.Rank;
        int opponentKingFile = friendlyKingSquare.File;
        int opponentKingDstToCentreFile = Math.Max(3 - opponentKingFile, opponentKingFile - 4);
        int opponentKingDstToCentreRank = Math.Max(3 - opponentKingRank, opponentKingRank - 4);
        int opponentKingDstFromCentre = opponentKingDstToCentreFile + opponentKingDstToCentreRank;
        evaluation += opponentKingDstFromCentre;
        int friendlyKingRank = friendlyKingSquare.Rank;
        int friendlyKingFile = friendlyKingSquare.File;
        int dstBetweenKingsFile = Math.Abs(friendlyKingFile - opponentKingFile);
        int dstBetweenKingsRank = Math.Abs(friendlyKingRank - opponentKingRank);
        int dstBetweenKings = dstBetweenKingsFile + dstBetweenKingsRank;
        evaluation += 14 - dstBetweenKings;
        return (int)(evaluation * 10 * endgameWeight);
    }

    int Evaluate(Board board, Move move)
    {
        bool player = board.IsWhiteToMove;
        Random rng = new Random();
        int eval = 0;
        int randomFactor = rng.Next(-5, 5);
        int endgameScore = ForceKingToCornerEndgameEval(board.GetKingSquare(player), board.GetKingSquare(!player), map_value(board.PlyCount, 0, 60, 0, 1));
        eval = materialEval(board) + randomFactor + endgameScore;
        int drawEval = Convert.ToInt32(board.IsDraw()) * -eval * 100;
        return eval + drawEval;
    }

    /*int Search (Board board, int depth, int alpha, int beta, Move lastMove) {
        if (depth == 0) {
            return Evaluate(board, lastMove);
        }
        Move[] moves = board.GetLegalMoves();
        if (board.IsInCheckmate())
        {
            return int.MinValue;
        }
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int evaluation = -Search(board, depth - 1, alpha, beta, move);
            board.UndoMove(move);
            if (evaluation >= beta)
            {
                // Move was too good, opponent will avoid this position 
                return beta; // *Snip*
            }
            alpha = Math.Max(alpha, evaluation);
        }
        return alpha;
    }*/

    int Minimax(Board board, int depth, float alpha, float beta, bool maximizingPlayer, Move lastMove)
    {
        Move[] moves = board.GetLegalMoves();
        if (depth == 0)
        {
            return Evaluate(board, lastMove);
        }
        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Move move in moves)
            {
                Board previousBoard = board;
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, alpha, beta, false, lastMove);
                board.UndoMove(move);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, maxEval);
                if (beta <= alpha) break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Move move in moves)
            {
                Board previousBoard = board;
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, alpha, beta, true, lastMove);
                board.UndoMove(move);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }

    Move chooseMove(Board board, int depth)
    {
        List<int> scores = new List<int>();
        Move[] moves = orderMoves(board, board.GetLegalMoves());
       // Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            scores.Add(Minimax(board, depth, int.MinValue, int.MaxValue, true, move));
            board.UndoMove(move);
        }
        return moves[scores.IndexOf(scores.Min())];
    }
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return chooseMove(board, 2);
    }
}