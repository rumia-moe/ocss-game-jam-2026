using TMPro;
using UnityEngine;

public class InfoHandler : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI healthValue;
    public TextMeshProUGUI attackValue;
    public TextMeshProUGUI insightValue;
    public TextMeshProUGUI agilityValue;
    public TextMeshProUGUI intellectValue;


    public void ChangeInfo(Entity muse)
    {
        title.text = muse.EntityName;
        healthValue.text = muse.EntityMaxHealth.ToString();
        attackValue.text = muse.damage.ToString();
        insightValue.text = muse.insight.ToString();
        agilityValue.text = muse.movementSpeed.ToString();
        intellectValue.text = muse.intelect.ToString(); 
    }



}
