using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class Example : MonoBehaviour
{
    [SerializeField]
    private GameObject _obj;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // クリックしたスクリーン座標
            var screenPoint = Input.mousePosition;

            // Canvasにセットされているカメラを取得
            var graphic = GetComponent<Graphic>();
            var camera = graphic.canvas.worldCamera;

            // Overlayの場合はScreenPointToLocalPointInRectangleにnullを渡さないといけないので書き換える
            if (graphic.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                camera = null;
            }

            // クリック位置に対応するRectTransformのlocalPositionを計算する
            var localPoint = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(graphic.rectTransform, screenPoint, camera, out localPoint);

            // ローカル座標にインスタンス生成
            var instance = Instantiate(_obj, transform);
            instance.GetComponent<RectTransform>().localPosition = localPoint;
            instance.SetActive(true);
        }
    }
}