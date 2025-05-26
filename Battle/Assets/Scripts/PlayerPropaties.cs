using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

public static class PlayerPropaties
{
    private const string DeadKey = "Dead";
    private const string ScoreKey = "Score";
    private const string RankKey = "Rank";

    private static readonly Hashtable playerHash = new Hashtable();

    // 死亡の真偽を設定
    public static void SetDead(this Player player, bool dead)
    {
        playerHash[DeadKey] = dead;
        player.SetCustomProperties(playerHash);
        playerHash.Clear();
    }

    // 死亡の真偽を取得
    public static bool GetDead(this Player player)
    {
        if (player.CustomProperties.TryGetValue(DeadKey, out object value) && value is bool dead)
        {
            return dead;
        }
        else
        {
            Debug.LogError("[Dead]のカスタムプロパティを取得できませんでした");
            return false;
        }
    }

    // スコアを設定
    public static void SetScore(this Player player, int score)
    {
        playerHash[ScoreKey] = score;
        player.SetCustomProperties(playerHash);
        playerHash.Clear();
    }

    // スコアを取得
    public static int GetScore(this Player player)
    {
        if (player.CustomProperties.TryGetValue(ScoreKey, out object value) && value is int score)
        {
            return score;
        }
        else
        {
            Debug.LogError("[Score]のカスタムプロパティを取得できませんでした");
            return 0;
        }
    }

    // スコアを加算
    public static void AddScore(this Player player, int score)
    {
        playerHash[ScoreKey] = player.GetScore() + score;
        player.SetCustomProperties(playerHash);
        playerHash.Clear();
    }

    // 順位を設定
    public static void SetRank(this Player player, int rank)
    {
        playerHash[RankKey] = rank;
        player.SetCustomProperties(playerHash);
        Debug.Log($"順位を{playerHash[RankKey]}に設定");
        playerHash.Clear();
    }

    // 順位を取得
    public static int GetRank(this Player player)
    {
        if (player.CustomProperties.TryGetValue(RankKey, out object value) && value is int rank)
        {
            return rank;
        }
        else
        {
            Debug.LogError("[Rank]のカスタムプロパティを取得できませんでした");
            return 0;
        }
    }
}
