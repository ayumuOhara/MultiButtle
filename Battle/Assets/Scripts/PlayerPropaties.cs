using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

public static class PlayerPropaties
{
    private const string DeadKey = "Dead";
    private const string ScoreKey = "Score";
    private const string RankKey = "Rank";

    private static readonly Hashtable playerHash = new Hashtable();

    // ���S�̐^�U��ݒ�
    public static void SetDead(this Player player, bool dead)
    {
        playerHash[DeadKey] = dead;
        player.SetCustomProperties(playerHash);
        playerHash.Clear();
    }

    // ���S�̐^�U���擾
    public static bool GetDead(this Player player)
    {
        if (player.CustomProperties.TryGetValue(DeadKey, out object value) && value is bool dead)
        {
            return dead;
        }
        else
        {
            Debug.LogError("[Dead]�̃J�X�^���v���p�e�B���擾�ł��܂���ł���");
            return false;
        }
    }

    // �X�R�A��ݒ�
    public static void SetScore(this Player player, int score)
    {
        playerHash[ScoreKey] = score;
        player.SetCustomProperties(playerHash);
        playerHash.Clear();
    }

    // �X�R�A���擾
    public static int GetScore(this Player player)
    {
        if (player.CustomProperties.TryGetValue(ScoreKey, out object value) && value is int score)
        {
            return score;
        }
        else
        {
            Debug.LogError("[Score]�̃J�X�^���v���p�e�B���擾�ł��܂���ł���");
            return 0;
        }
    }

    // �X�R�A�����Z
    public static void AddScore(this Player player, int score)
    {
        playerHash[ScoreKey] = player.GetScore() + score;
        player.SetCustomProperties(playerHash);
        playerHash.Clear();
    }

    // ���ʂ�ݒ�
    public static void SetRank(this Player player, int rank)
    {
        playerHash[RankKey] = rank;
        player.SetCustomProperties(playerHash);
        Debug.Log($"���ʂ�{playerHash[RankKey]}�ɐݒ�");
        playerHash.Clear();
    }

    // ���ʂ��擾
    public static int GetRank(this Player player)
    {
        if (player.CustomProperties.TryGetValue(RankKey, out object value) && value is int rank)
        {
            return rank;
        }
        else
        {
            Debug.LogError("[Rank]�̃J�X�^���v���p�e�B���擾�ł��܂���ł���");
            return 0;
        }
    }
}
