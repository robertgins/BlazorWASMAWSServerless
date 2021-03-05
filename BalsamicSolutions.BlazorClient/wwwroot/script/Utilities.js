//Simple user inactivity timer
function initializeInactivityTimer(dotnetHelper, timeOutInMinutes) {

    var inactivityCounter = 0;
    document.onmousemove = resetTimer;
    document.onkeypress = resetTimer;
    function resetTimer() {
        inactivityCounter = 0;
    }
    setInterval(checkActivity, 30000);
    function checkActivity() {
        dotnetHelper.invokeMethodAsync("IsAuthenticated").then(isAuthenticated => {
            if (isAuthenticated) {
                inactivityCounter++;
                if (inactivityCounter / 2 >= timeOutInMinutes) {
                    inactivityCounter = 0;
                    dotnetHelper.invokeMethodAsync("LogoutFromInactivityTimer")
                }
            }
            else {
                inactivityCounter = 0;
            }
        });

    }
}

//Save dialog
function FileSaveAs(filename, fileContent) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:text/plain;charset=utf-8," + encodeURIComponent(fileContent)
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}