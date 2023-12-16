namespace TgFileClient;

/// <summary>
///  Represents a callback that is called when a file is being uploaded or downloaded.
/// </summary>
/// <param name="transferred">Number of bytes transferred</param>
/// <param name="total">Total number of bytes to transfer</param>
/// <param name="progress">Progress percentage</param>
public delegate void ProgressCallback(long transferred, long total, double progress);