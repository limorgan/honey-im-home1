using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CustomReaction : MonoBehaviour
{
    public KeyValuePair<string, bool> property = new KeyValuePair<string, bool>();
    public Animation animation;
    private Property _prop;

    public void PlayAnimation()
    {
        _prop = this.gameObject.GetComponent<GameItem>().GetProperty(property.Key);
        if (_prop != null)
            if (_prop.value.ToString() == property.Value.ToString().ToLower())
            {
                animation.Play();
                StartCoroutine(WaitUntilEndOfAnimation(animation));
            }
    }

    IEnumerator WaitUntilEndOfAnimation(Animation animation) {
        yield return new WaitForSeconds(animation.clip.length);
    }
}
