using System;

[Serializable]
public class PlayerProfile
{
    public string profileId;
    public string playerName;
    public int age;
    public string gender;
    public string creationDate;

    public PlayerProfile(string name, int age, string gender)
    {
        this.profileId = Guid.NewGuid().ToString();
        this.playerName = name;
        this.age = age;
        this.gender = gender;
        this.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

[Serializable]
public class PlayerProfileData
{
    public PlayerProfile[] profiles;
}
