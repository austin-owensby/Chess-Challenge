using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    static readonly Random rng = new();

    public Move Think(Board board, Timer timer)
    {
        return ThinkAtDepth(board, timer, 2).Move;
    }

    private MoveEvaluation ThinkAtDepth(Board board, Timer timer, int depth)
    {
        Span<Move> allMoves = stackalloc Move[256];
        board.GetLegalMovesNonAlloc(ref allMoves);

        // Pick a random move to play if nothing better is found
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        int maxEvaluation = 0;

        foreach (Move move in allMoves)
        {
            // Always play checkmate in one
            if (MoveIsCheckmate(board, move))
            {
                moveToPlay = move;
                maxEvaluation = 10000;
                break;
            }

            // Find highest value capture
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int evaluation = pieceValues[(int)capturedPiece.PieceType];

            if (MoveIsDraw(board, move))
            {
                evaluation = 0;
            }
            else if (depth > 1)
            {
                // Subtract the opponent's best evaluation
                board.MakeMove(move);
                evaluation -= ThinkAtDepth(board, timer, depth - 1).Evaluation;
                board.UndoMove(move);
            }

            if (evaluation > maxEvaluation)
            {
                moveToPlay = move;
                maxEvaluation = evaluation;
            }
        }

        return new() { 
            Move = moveToPlay,
            Evaluation = maxEvaluation
        };
    }

    // Test if this move gives checkmate
    static bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    static bool MoveIsDraw(Board board, Move move)
    {
        board.MakeMove(move);
        bool isDraw = board.IsDraw();
        board.UndoMove(move);
        return isDraw;
    }

    public class MoveEvaluation
    {
        public Move Move { get; set; }
        public int Evaluation { get; set; }
    }
}