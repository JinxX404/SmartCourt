$connStr = "Server=db58693.public.databaseasp.net; Database=db58693; User Id=db58693; Password=m-7XB4a?3#Ge; Encrypt=True; TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "UPDATE AspNetUsers SET EmailConfirmed = 1 WHERE Email = 'ahmedkokker9@gmail.com'"
$rows = $cmd.ExecuteNonQuery()
Write-Host "Successfully activated $rows account(s)"
$conn.Close()
