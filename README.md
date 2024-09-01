Ado - Administrator do, ©2024 - Manfred Mueller  
  
Ausführen eines Prozesses auf der Kommandozeile mit erweiterten Zugriffsrechten über UAC  
Verwendung: ado [-?|-wait|-k] prog [args]  
-?&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;- Zeigt diese Hilfe  
-wait&nbsp;&nbsp;&nbsp;- Wartet, bis prog beendet ist  
-k&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;- Startet den Wert der Umgebungsvariablen %COMSPEC% und  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;führt das darin enthaltene Programm aus (CMD.EXE, usw.)  
-&nbsp;&nbsp;i&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;- Installiert das Programm in die Programmdateien des aktuellen Benutzers und  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;fügt es der PATH-Variable des Benutzers hinzu.  
-u&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;- Deinstalliert das Programm aus den Programmdateien des aktuellen Benutzers und  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;entfernt es aus der PATH-Variable des Benutzers.  
prog&nbsp;&nbsp;&nbsp;&nbsp;- Das auszuführende Programm  
[args]&nbsp;&nbsp;- Optionale Befehlszeilenargumente für prog  
