using UnityEngine;
using System.Collections;

public class mapitro : MonoBehaviour {
    public static mapitro instance;
    public GameObject muvingship, CSSidie, CSSrun;
    public GameObject mapitroativer;
	// Use this for initialization
	void Start () {
        instance = this;

    }
  public	void Muvingship()
    {
        if (CSSidie != null) CSSidie.SetActive(false);
        if (muvingship != null)
        {
            ShipMuvinginItro shipMover = muvingship.GetComponent<ShipMuvinginItro>();
            if (shipMover != null) shipMover.enabled = true;
        }
    }
	 public void HidemapItro()
    {
        if (mapitroativer != null) mapitroativer.SetActive(false);
    }
}
