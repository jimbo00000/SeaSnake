using UnityEngine;

internal class ButtonTrigger : MonoBehaviour
{
    /// <summary>
    /// id of this button
    /// </summary>
    public int ID { get; set; }

    private bool hover;

    private void Update()
    {
        RaycastHit hit;

        var oldHover = hover;

        if (collider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
        {
            hover = true;
        }
        else
        {
            hover = false;
        }

        if (Input.GetMouseButtonDown(0) && hover)
        {
            Demo.Instance.OnButtonHit(ID);
        }
        else if (hover != oldHover)
        {
            Demo.Instance.OnButtonHover(ID, hover);
        }
    }
}
