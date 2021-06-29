using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TagItemObject : MonoBehaviour
{
    // Start is called before the first frame update

    public Button deleteButton;
    public InputField tagName;
    public MainWorker worker;
    public bool isAcceptPanel;
    public int index;
    public void OnClick()
    {
        worker.DeleteItemAt(index, isAcceptPanel,gameObject);
    }

    public void OnChanged()
    {
        worker.ReloadArray(index, isAcceptPanel, tagName.text);
    }
    
    // void Start()
    // {
    //     
    // }
    //
    // // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
