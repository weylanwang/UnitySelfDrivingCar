using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class SliderReader : MonoBehaviour
{
    public Slider slider;
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        string value = slider.value.ToString();
        value = value.Substring(0, (4 < value.Length ? 4 : value.Length));
        if (value == "0.00")
            value = "0";
        text.text = value;
    }
}
