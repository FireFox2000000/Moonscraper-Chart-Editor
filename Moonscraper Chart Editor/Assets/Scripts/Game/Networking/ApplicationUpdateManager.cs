using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Octokit;

public class ApplicationUpdateManager
{
#if UNITY_EDITOR
    readonly string productHeader = "MoonscraperChartEditor_Dev";
#else
    readonly string productHeader = "MoonscraperChartEditor";
#endif
    readonly string respositoryOwner = "FireFox2000000";
    readonly string repositoryName = "Moonscraper-Chart-Editor";

    public string currentVersion { get; private set; }
    public bool UpdateCheckInProgress { get; private set; }

    public delegate void OnUpdateFoundFn(Release release);

    public ApplicationUpdateManager(string currentVersion)
    {
        this.currentVersion = currentVersion;
    }

    public async void CheckForUpdates(OnUpdateFoundFn onUpdateFoundCallback, bool allowPreleases = false)
    {
        if (UpdateCheckInProgress)
        {
            Debug.LogError("Trying to check for an update when checking is already in progress.");
            return;
        }

        UpdateCheckInProgress = true;

        try
        {
            var github = new GitHubClient(new ProductHeaderValue(productHeader));

            // Safety first, repo may be down or cannot be reached
            try
            {
                var releases = await github.Repository.Release.GetAll(respositoryOwner, repositoryName);
                if (releases != null && releases.Count > 0)
                {
                    Release latest = null;   
                    
                    foreach(var release in releases)
                    {
                        if (!release.Prerelease || allowPreleases)
                        {
                            latest = release;
                            break;
                        }
                    }

                    if (latest != null)
                    {
                        Debug.Log("Found latest release on GitHub. Release version is " + latest.TagName);
#if UNITY_EDITOR
                        Debug.Assert(!IsLatestVersionNewer(currentVersion, latest.TagName), "Development version number is considered to be an earlier version of the current release. Please fix the version number or update your working copy.");
#endif

                        if (!IsLatestVersionNewer(currentVersion, latest.TagName))
                        {
                            latest = null;  // Already on the latest version
                            Debug.Log("Application is considered to be the same or newer than the latest release. Current version is " + currentVersion);
                        }
                    }

                    onUpdateFoundCallback(latest);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Unable to obtain information for the latest release: " + e.Message);
            }
        }
        catch (Exception e)     // I don't trust that github will be around forever. Plus users may not even be connected to the internet.
        {
            Debug.LogWarning("Unable to create github client: " + e.Message);
        }

        UpdateCheckInProgress = false;
    }

    static bool IsLatestVersionNewer(string currentVersion, string latest)
    {
        return string.CompareOrdinal(currentVersion, latest) < 0;
    }
}
