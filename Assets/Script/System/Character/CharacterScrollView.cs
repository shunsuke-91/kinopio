using UnityEngine;
using UnityEngine.UI;
using System;

public class CharacterScrollView : MonoBehaviour
{
    [SerializeField] private Transform content;              // ScrollView の Content
    [SerializeField] private GameObject characterButtonPrefab; // ボタンのプレハブ

    /// <summary>
    /// キャラボタンが押されたときに通知するイベント
    /// （外側のスクリプトから購読して使う）
    /// </summary>
    public Action<CharacterInstance> OnCharacterSelected;

    /// <summary>
    /// 所持キャラ一覧を ScrollView に並べ直す
    /// </summary>
    public void Refresh()
    {
        if (content == null || characterButtonPrefab == null)
        {
            Debug.LogError("CharacterScrollView: content か characterButtonPrefab が設定されていません");
            return;
        }

        if (CharacterManager.Instance == null)
        {
            Debug.LogError("CharacterScrollView: CharacterManager.Instance が見つかりません");
            return;
        }

        // いったん中身をクリア
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 所持キャラを順番にボタンとして生成
        foreach (var character in CharacterManager.Instance.ownedCharacters)
        {
            // ボタン生成
            var btnObj = Instantiate(characterButtonPrefab, content);

            // もしボタン内に Text があれば、とりあえず ToString() を表示しておく
            // （CharacterInstance が ToString をオーバーライドしていなければ
            //  後で好きな表示に差し替えてください）
            var text = btnObj.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = character != null ? character.ToString() : "キャラ";
            }

            // ボタン押下時の処理を登録
            var button = btnObj.GetComponent<Button>();
            if (button != null)
            {
                var captured = character; // クロージャ対策
                button.onClick.AddListener(() =>
                {
                    Debug.Log("選択されたキャラ: " + captured);
                    OnCharacterSelected?.Invoke(captured);
                });
            }
        }
    }
}