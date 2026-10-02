param([string]$sqlFile)
$content = Get-Content -Raw $sqlFile
$conn = New-Object System.Data.SqlClient.SqlConnection("Data Source=.;Initial Catalog=FGA;Integrated Security=True")
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $content
    $res = $cmd.ExecuteNonQuery()
    Write-Output "SUCCESS: Script executed successfully."
} catch {
    Write-Output ("ERROR: " + $_.Exception.Message)
} finally {
    $conn.Close()
}
