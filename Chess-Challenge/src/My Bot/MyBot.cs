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

    int evaluated = 0;


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

    float score(float w, float d, float l)
    {
        float t = w + d + l;
        return (((w - l + (d / 2) + t) * 10) / (2 * t));
    }

    int completionEval(Board board, int currentEval)
    {
        if (board.IsDraw()) return -currentEval * 500;
        return 0;
    }

    int captureEval(Board board, Move move)
    {
        int captureValue = 0;
        if (move.IsCapture) captureValue += pieceTypeValue(move.CapturePieceType) - pieceTypeValue(move.MovePieceType);
        return boolToInt(board.IsWhiteToMove) * captureValue;
    }

    int Evaluate(Board board, Move move)
    {
        evaluated++;
        Random rng = new Random();
        int randomFactor = rng.Next(-5, 5);
        int evaluation = 0;
        evaluation += materialEval(board) + randomFactor;
        //evaluation += completionEval(board, evaluation);
        return evaluation;
    }

    int Search (Board board, int depth, int alpha, int beta, Move lastMove) {
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
    }

    Move chooseMove(Board board, int depth)
    {
        List<int> scores = new List<int>();
        Move[] moves = board.GetLegalMoves();
        foreach(Move move in moves)
        {
            evaluated = 0;
            board.MakeMove(move);
            scores.Add(Search(board, depth, int.MinValue, int.MaxValue, move));
            Console.WriteLine(move.StartSquare.Name + move.TargetSquare.Name + " : " + scores.Last() + " | evaluated: " + evaluated);
            board.UndoMove(move);
        }
        Console.WriteLine("--------------------------------");
        return moves[scores.IndexOf(scores.Min())];
    }
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Console.WriteLine(score(39, 46, 13));
        return chooseMove(board, 3);
    }
}