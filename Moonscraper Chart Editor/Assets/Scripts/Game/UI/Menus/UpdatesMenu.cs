using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdatesMenu : MonoBehaviour
{
    [SerializeField]
    Button downloadLatestButton;
    [SerializeField]
    Text textInfo;
    [SerializeField]
    TextAsset versionNumber;
    [SerializeField]
    Toggle checkUpdateToggle;

    public void Populate(Octokit.Release latestRelease)
    {
        string currentVersion = versionNumber.text;

        bool hasNewerVersion = latestRelease != null;
        downloadLatestButton.gameObject.SetActive(hasNewerVersion);

        if (hasNewerVersion && latestRelease.Assets.Count > 0)
        {
            textInfo.text = string.Format("A new version of Moonscraper Chart Editor is available.\n\nCurrent version- {0}\nLatest version- {1}", currentVersion, latestRelease.Name);

            var downloadAsset = latestRelease.Assets[0];

            downloadLatestButton.onClick.RemoveAllListeners();
            downloadLatestButton.onClick.AddListener(() =>
            {
                Application.OpenURL(downloadAsset.BrowserDownloadUrl);
            });
        }
        else
        {
            textInfo.text = string.Format("Moonscraper Chart Editor is up to date. The current version is {0}", currentVersion);         
        }
    }

    private void OnEnable()
    {
        checkUpdateToggle.isOn = GameSettings.automaticallyCheckForUpdates;
    }

    public void OnAutoCheckUpdatesToggles(bool updatesEnabled)
    {
        GameSettings.automaticallyCheckForUpdates = updatesEnabled;
    }
}
