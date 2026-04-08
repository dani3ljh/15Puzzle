using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Represents a Conformation UI Window
/// </summary>
public class ConfirmationWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text header;
    [SerializeField] private TMP_Text subHeader;
    
    private Action yesCallBack;
    private Action noCallBack;

    public void SetHeaders(string headerText, string subHeaderText = "")
    {
        header.text = headerText;
        subHeader.text = subHeaderText;
        // print($"set header to \"{headerText}\", and subheader to \"{subHeaderText}\"");
    }
    
    public void SetCallbacks(Action yes, Action no)
    {
        yesCallBack = yes;
        noCallBack = no;
    }
    
    public void NoButtonPressed() => noCallBack?.Invoke();
    public void YesButtonPressed() => yesCallBack?.Invoke();
}