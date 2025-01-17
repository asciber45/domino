﻿using System.Collections;

namespace DominoEngine;

public class Game<T> : IEnumerable<GameState<T>>, IWinneable<T> {
    private readonly Judge<T>? _judge; // Juez que guiara este game por completo
    private readonly Partida<T>? _partida;

    private Game(Judge<T> judge, IEnumerable<Team<T>> teams) {
        _judge = judge;
        _partida = new Partida<T>(teams);
    }

    public Game() { }

    public IEnumerator<GameState<T>> GetEnumerator() {
        _judge!.Start(_partida!); // Se preparan las condiciones para comenzar el Game
        // Crear el primer GameState, antes de la primera jugada
        var firstState = new GameState<T>(_partida!.Board.ToList(), _partida.Hands);
        // Devolver los GameState uno a uno mientras se efectuan las jugadas
        return Enumerable.Repeat(firstState, 1).Concat(_judge.Play(_partida).
        Select((player, i) => new GameState<T>(_partida.Board.ToList(), _partida.Hands, i, player))).GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Devuelve el winner que declare el juez
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Team<T>> Winner() => _judge!.Winner(_partida!);

    public IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams) => new Game<T>(judge, teams);

    /// <summary>
    /// Devuelve un IEnumerable de un elemento de si mismo
    /// </summary>
    /// <param name="winneable"></param>
    /// <returns></returns>
    public IEnumerable<Game<T>> Games(IWinneable<T> winneable) => Enumerable.Repeat(this, 1);
}

// Representa toda la informacion necesaria para un expectador luego de cada jugada
public record GameState<T>(List<Move<T>> Board, Dictionary<Player<T>, Hand<T>> Hands,
    int Turn = -1, Player<T> PlayerToPlay = default!) {
    public override string ToString()
    {
        var result = "";
        if (Turn is not -1) {
            result += $"Turn: {Turn}\n";
            result += $"Player to Play: {PlayerToPlay.ToString()}\n";
        }
        result += $"Hands:\n";
        foreach (var (player, hand) in Hands)
            result += $"Player {player.ToString()}: {hand.ToString()}\n";
        var count = 0;
        return Board.Aggregate(result, (current, move) => current + $"Turn {count++}: Player {move.PlayerId}: {move.ToString()}\n");
    }
}
