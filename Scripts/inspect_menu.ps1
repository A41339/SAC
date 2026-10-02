$conn = New-Object System.Data.SqlClient.SqlConnection("Data Source=.;Initial Catalog=FGA;Integrated Security=True")
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
SELECT Id, MenuText, MenuURL, ParentId, SortOrder, Description 
FROM dbo.Menu 
WHERE Id IN (6000, 6001, 6002, 6003, 6004, 6005, 9000, 9135, 9300)
   OR ParentId IN (9000, 9300, 6000)
   OR MenuText LIKE '%SBR%' 
   OR MenuText LIKE '%Avance%' 
   OR MenuText LIKE '%Historial%' 
   OR MenuURL LIKE '%Evaluac%'
ORDER BY ISNULL(ParentId, 0), SortOrder, Id
"@
    $da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dt = New-Object System.Data.DataTable
    $da.Fill($dt) | Out-Null
    $dt | Format-Table -AutoSize | Out-String | Write-Output
} catch {
    Write-Output ("ERROR: " + $_.Exception.Message)
} finally {
    $conn.Close()
}
