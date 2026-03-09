using System.Collections.Generic;
using UnityEngine;

public class BattleDialogueDatabase
{
    private readonly Dictionary<string, BattleDialogueData> _byId = new Dictionary<string, BattleDialogueData>();
    private readonly Dictionary<string, BattleDialogueData> _byEnemyAndKey = new Dictionary<string, BattleDialogueData>();

    public void Load(string csvFileName)
    {
        _byId.Clear();
        _byEnemyAndKey.Clear();

        List<Dictionary<string, object>> parsedData = CSVParser.Read(csvFileName);

        if (parsedData == null || parsedData.Count == 0)
        {
            Debug.LogError($"[BattleDialogueDatabase] CSV 파싱 실패 또는 데이터 없음: {csvFileName}");
            return;
        }

        foreach (var row in parsedData)
        {
            BattleDialogueData data = new BattleDialogueData
            {
                ID = row.ContainsKey("ID") ? row["ID"].ToString() : "",
                EnemyID = row.ContainsKey("EnemyID") ? row["EnemyID"].ToString() : "",
                Key = row.ContainsKey("Key") ? row["Key"].ToString() : "",
                Speaker = row.ContainsKey("Speaker") ? row["Speaker"].ToString() : "",
                Text = row.ContainsKey("Text") ? row["Text"].ToString() : "",
                PortraitName = row.ContainsKey("Portrait") ? row["Portrait"].ToString() : "",
                NextKey = row.ContainsKey("NextKey") ? row["NextKey"].ToString() : ""
            };

            if (!string.IsNullOrEmpty(data.ID))
                _byId[data.ID] = data;

            string compositeKey = MakeCompositeKey(data.EnemyID, data.Key);
            if (!string.IsNullOrEmpty(data.EnemyID) && !string.IsNullOrEmpty(data.Key))
                _byEnemyAndKey[compositeKey] = data;
        }

        Debug.Log($"[BattleDialogueDatabase] 로드 완료: {_byId.Count}건");
    }

    public BattleDialogueData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        _byId.TryGetValue(id, out var data);
        return data;
    }

    public BattleDialogueData GetByEnemyAndKey(string enemyId, string key)
    {
        if (string.IsNullOrEmpty(enemyId) || string.IsNullOrEmpty(key))
            return null;

        _byEnemyAndKey.TryGetValue(MakeCompositeKey(enemyId, key), out var data);
        return data;
    }

    private string MakeCompositeKey(string enemyId, string key)
    {
        return $"{enemyId}::{key}";
    }

    public void Clear()
    {
        _byId.Clear();
        _byEnemyAndKey.Clear();
    }
}