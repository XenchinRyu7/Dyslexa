/// <summary>
/// Interface untuk semua panel yang mendukung fitur Hint.
/// GameSessionManager cukup GetComponent<IHintable>().ShowHint()
/// tanpa perlu tau tipe panel yang aktif.
/// </summary>
public interface IHintable
{
    void ShowHint();
}
