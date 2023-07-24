using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    // TODO: Implement a transposition table 
    Dictionary<ulong, double> transpositionTable = new Dictionary <ulong, double>();

    // This is a surprise tool that will help us later
    int depth = 10;

    /*
    What each piece is worth starting
    order: pawn, knight, bishop, rook, queen, king
    */
    double[] value = {1f, 3f, 3.25f, 5f, 9f, 100f};
    public Move Think(Board board, Timer timer)
    {
        (Move, double) position = AbPruning(board, timer, board.IsWhiteToMove, depth, this.transpositionTable, double.MinValue, double.MaxValue);
        Console.WriteLine(position.Item2);
        return position.Item1;
        
    }

    //return the "best" move with its evaluation
    // Change later
    public (Move, double) AbPruning(Board board, Timer timer, bool isWhite, int depth, Dictionary<ulong, double> table, double alpha, double beta){

        double mul = board.IsWhiteToMove ? 1 : -1;
        PriorityQueue<(Move, double, bool), double> queue = new PriorityQueue<(Move, double, bool), double>();

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            ulong key = board.ZobristKey;
            double eval;
            if (table.ContainsKey(key))
            {
                eval = table[key];
            }
            else
            {
                eval = evaluatePosition(board);
                table.Add(key, eval);
            }
            bool isCheckmate = board.IsInCheckmate();
            board.UndoMove(move);

            if (isCheckmate)
            {
                if (isWhite == board.IsWhiteToMove)
                {
                    return (move,eval);
                }
            } else
            {
                queue.Enqueue((move, eval, isCheckmate), eval * -mul);
            }
        }

        Move bestMove = Move.NullMove;
        if (queue.Count > 0)
        {
            bestMove = queue.Peek().Item1;
        }
        double bestVal = board.IsWhiteToMove ? double.MinValue : double.MaxValue;
        int to = queue.Count < 10 ? 0 : queue.Count - 10;
        while (queue.Count > to)
        {
            (Move, double, bool) elem = queue.Dequeue();
            double res = elem.Item2;

            board.MakeMove(elem.Item1);
            if (depth > 0)
            {
                
                (Move, double) a = AbPruning(board, timer, isWhite, depth - 1, table, alpha, beta);
                res = a.Item2;
            }
            board.UndoMove(elem.Item1);

            if (bestVal * mul < res * mul)
            {
                bestMove = elem.Item1;
                bestVal = res;
            }

            if (board.IsWhiteToMove)
            {
                alpha = alpha < bestVal ? bestVal : alpha;
            } else
            {
                beta = beta > bestVal ? bestVal : beta;
            }

//  || (timer.MillisecondsElapsedThisTurn > 2000)
            if (beta <= alpha) break;
        }
        return (bestMove, bestVal);
    }

    
        // Change this later
    public double evaluatePosition(Board board){

        double white = board.GetPieceList(PieceType.Pawn, true).Count * value[0] + board.GetPieceList(PieceType.Knight, true).Count * value[1] + board.GetPieceList(PieceType.Bishop, true).Count * value[2]
        + board.GetPieceList(PieceType.Rook, true).Count * value[3] + board.GetPieceList(PieceType.Queen, true).Count * value[4];

        double black = board.GetPieceList(PieceType.Pawn, false).Count * value[0] + board.GetPieceList(PieceType.Knight, false).Count * value[1] + board.GetPieceList(PieceType.Bishop, false).Count * value[2]
        + board.GetPieceList(PieceType.Rook, false).Count * value[3] + board.GetPieceList(PieceType.Queen, false).Count * value[4];

    //    double white = Board.GetPieceList(PieceType.Pawn, true).Count * value[1] + Board.GetPieceList(PieceType.Knight, true).Count * value[2] + Board.GetPieceList(PieceType.Bishop, true).Count * value[3]
    //    + Board.GetPieceList(PieceType.Queen, true).Count * value[4];
    //     double black = Board.GetPieceList(PieceType.Pawn, false).Count * value[1] + Board.GetPieceList(PieceType.Knight, false).Count * value[2] + Board.GetPieceList(PieceType.Bishop, false).Count * value[3]
    //    + Board.GetPieceList(PieceType.Queen, false).Count * value[4];

       return (white - black);
    }
}