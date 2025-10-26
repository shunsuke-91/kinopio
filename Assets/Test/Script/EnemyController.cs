using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] enemys;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(EnemySpawn());
    }
    
    //一定間隔で出てくるようにしたい 

    IEnumerator EnemySpawn()
    {
        yield return new WaitForSeconds(5f);

        while (GameManager.instance.isGame)
        {
            int r = Random.Range(0, enemys.Length);

            float y = Random.Range(-0.69f, -1.69f);
            SpriteRenderer enemy = Instantiate(enemys[r], new Vector3(-6.23f, y, 0), transform.rotation);
            enemy.sortingOrder = (int)(-y * 10);

            yield return new WaitForSeconds(10f);
        }
    }
}
