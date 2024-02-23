using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateBar : MonoBehaviour
{
    private GameObject championGO;
    private ChampionController championController;
    public Image armorFillImage;
    public Image hpFillImage;
    public Image manaFillImage;

    private CanvasGroup canvasGroup;

    /// Start is called before the first frame update
    void Start()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();
    }

    /// Update is called once per frame
    void Update()
    {
        GetComponent<Transform>().eulerAngles =
            new Vector3(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, 0);

        if (championGO != null)
        {
            this.transform.position = championGO.transform.position + new Vector3(0, 1.5f + 1.5f * championGO.transform.localScale.x, 0);
            armorFillImage.fillAmount = championController.attributesController.curArmor
            / championController.attributesController.maxArmor.GetTrueValue();

            hpFillImage.fillAmount = championController.attributesController.curHealth
            / championController.attributesController.maxHealth.GetTrueValue();

            manaFillImage.fillAmount = championController.attributesController.curMana
            / championController.attributesController.maxMana.GetTrueValue();

            if (championController.attributesController.curHealth <= 0)
                canvasGroup.alpha = 0;
            else
                canvasGroup.alpha = 1;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Called when champion created
    /// </summary>
    /// <param name="_championGO"></param>
    public void Init(GameObject _championGO)
    {
        championGO = _championGO;
        championController = championGO.GetComponent<ChampionController>();

    }
}
