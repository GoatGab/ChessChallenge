using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

public class OldBot : IChessBot
{
    /*int posInfinity = int.MaxValue;
    int negInfinity = int.MinValue;

    int boolToInt(bool var)
    {
        if (var) return 1;
        return -1;
    }

    int GetPieceValue(PieceType pieceType)
    {
        List<PieceType> values = Enum.GetValues(typeof(PieceType))
                            .Cast<PieceType>()
                            .ToList();
        return values.IndexOf(pieceType);
    }

    Move[] orderMoves(Board board, Move[] moves)
    {
        int[] moveScoreGuesses = new int[moves.Length];
        int i = 0;
        foreach(Move move in moves)
        {
            if(move.CapturePieceType != PieceType.None) moveScoreGuesses[i] = 10 * GetPieceValue(move.CapturePieceType) - GetPieceValue(move.MovePieceType);
            if (move.IsPromotion) moveScoreGuesses[i] += GetPieceValue(move.PromotionPieceType);
            if(board.SquareIsAttackedByOpponent(move.TargetSquare)) moveScoreGuesses[i] -= 5*GetPieceValue(move.MovePieceType);
            i++;
        }
        List<Move> sortedMoves = moves.ToList();
        sortedMoves = sortedMoves.OrderBy(o => moveScoreGuesses).ToList();
        return sortedMoves.ToArray();
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

    int getMaterialCount(Board board, bool player)
    {
        return 10000 * board.GetPieceList(PieceType.King, player).Count +
        900 * board.GetPieceList(PieceType.Queen, player).Count +
        500 * board.GetPieceList(PieceType.Rook, player).Count +
        300 * board.GetPieceList(PieceType.Bishop, player).Count +
        300 * board.GetPieceList(PieceType.Knight, player).Count +
        100 * board.GetPieceList(PieceType.Pawn, player).Count;
    }

    int Eval(Board board, Board prevBoard)
    {
        Random rng = new Random();
        bool player = board.IsWhiteToMove;
        if (board.IsInCheckmate()) return (posInfinity * boolToInt(!player));
        int MaterialScore = getMaterialCount(board, player) - getMaterialCount(board, !player);
        int captureValue = (getMaterialCount(prevBoard, player) - getMaterialCount(board, player)) - (getMaterialCount(prevBoard, !player) - getMaterialCount(board, !player));
        int endgameScore = ForceKingToCornerEndgameEval(board.GetKingSquare(player), board.GetKingSquare(!player), map_value(board.PlyCount, 0, 60, 0, 1));
        return (MaterialScore + 10*captureValue + endgameScore)  * boolToInt(!player) + rng.Next(-5, 5);
    }

    /*float searchAllCaptures(Board board, float alpha, float beta, Board prevBoard)
    {
        float eval = Eval(board, prevBoard, 0);
        Console.WriteLine(eval);
        if (eval >= beta) return beta;
        alpha = Math.Max(alpha, eval);
        Move[] captureMoves = board.GetLegalMoves(true);
        captureMoves = orderMoves(board, captureMoves);
        foreach (Move captureMove in captureMoves)
        {
            Board prev = board;
            board.MakeMove(captureMove);
            eval = -searchAllCaptures(board, -beta, -alpha, prev);
            board.UndoMove(captureMove);
            if (eval >= beta) return beta;
            alpha = Math.Max(alpha, eval);
        }
        return alpha;
    }*/

    /*float Minimax(Board board, int depth, float alpha, float beta, bool maximizingPlayer, Board prevBoard)
    {
        Move[] moves = board.GetLegalMoves();
        if (depth == 0)
        { 
            return Eval(board, prevBoard);
        }
        if(maximizingPlayer)
        {
            float maxEval = negInfinity;
            foreach (Move move in moves)
            {
                Board previousBoard = board;
                board.MakeMove(move);
                float eval = Minimax(board, depth - 1, alpha, beta, false, previousBoard);
                board.UndoMove(move);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, maxEval);
                if (beta <= alpha) break;
            }
            return maxEval;
        } 
        else
        {
            float minEval = posInfinity;
            foreach (Move move in moves)
            {
                Board previousBoard = board;
                board.MakeMove(move);
                float eval = Minimax(board, depth - 1, alpha, beta, true, previousBoard);
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
        Move[] legalMoves = orderMoves(board, board.GetLegalMoves(false));

        float[] movesScore = new float[legalMoves.Length];
        for (int i = 0; i < legalMoves.Length; i++)
        {
            Board prevBoard = board;
            board.MakeMove(legalMoves[i]);
            movesScore[i] = MinMax(board, depth, int.MinValue, int.MaxValue, board.IsWhiteToMove, prevBoard);
            Console.WriteLine(legalMoves[i].StartSquare.Name + legalMoves[i].TargetSquare.Name + ":" + movesScore[i]);
            board.UndoMove(legalMoves[i]);
        }
        Console.WriteLine("-----------------");
        return legalMoves[movesScore.ToList().IndexOf(movesScore.Max())];
    }

    Board GenerateMovedBoard(Board oldBoard, Move move)
    {
        Board newBoard = oldBoard;
        newBoard.MakeMove(move);
        return newBoard;
    }

    int MinMax(Board board, int depth, int alpha, int beta, bool isMaximizingPlayer, Board prevBoard)
    {
        if (depth == 0)
            return Eval(board, prevBoard);

        if (isMaximizingPlayer)
        {
            int bestValue = int.MinValue;

            Move[] possibleMoves = board.GetLegalMoves();

            possibleMoves = orderMoves(board, possibleMoves.ToArray());
            foreach (Move move in possibleMoves)
            {
                Board prev = board;
                board.MakeMove(move);
                int value = MinMax(board, depth - 1, alpha, beta, false, prev);
                board.UndoMove(move);
                bestValue = Math.Max(value, bestValue);

                alpha = Math.Max(alpha, value);

                if (beta <= alpha)
                {
                    break;
                }
            }

            return bestValue;
        }
        else
        {
            int bestValue = int.MaxValue;

            Move[] possibleMoves = board.GetLegalMoves();
            possibleMoves = orderMoves(board, possibleMoves.ToArray());

            foreach (Move move in possibleMoves)
            {
                Board prev = board;
                board.MakeMove(move);
                int value = MinMax(board, depth - 1, alpha, beta, true, prev);
                board.UndoMove(move);
                bestValue = Math.Min(value, bestValue);

                beta = Math.Min(beta, value);

                if (beta <= alpha)
                {
                    break;
                }
            }

            return bestValue;
        }
    }
    */
    public Move Think(Board board, Timer timer)
    {
        return board.GetLegalMoves()[0];
    }
}