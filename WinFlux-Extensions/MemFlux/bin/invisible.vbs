Set objShell = CreateObject("Shell.Application")
Set objArgs = WScript.Arguments

If objArgs.Count >= 1 Then
    strProgram = objArgs(0)
    
    If objArgs.Count >= 2 Then
        objShell.ShellExecute strProgram, objArgs(1), "", "runas", 0
    Else
        objShell.ShellExecute strProgram, "", "", "runas", 0
    End If
End If