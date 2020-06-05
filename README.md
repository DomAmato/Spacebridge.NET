# Spacebridge.NET
Spacebridge client for Windows using C#

## Using the application

Running the application will start up this screen which requires your api key which is generated from our dashboard.

![Start Screen](/images/start_screen.png)

Once the key is entered click connect and assuming the key is correct you will be taken to the next screen

![Key Entered](/images/key_entered.png)

![Select Org](/images/select_org.png)

Selecting an org from the dropdown will populate the dropdown menus with possible devices for tunneling into. Hitting connect will start port forwarding given the values provided. It will take a bit for it to connect so wait until the dot turns green. Any errors will pop up

![Connected](/images/connected.png)

## System Tray

The app runs in the system tray so closing the window does not actually exit the application
![Tray App](/images/tray_app.png)

If you want to close the application right click on the system tray icon and hit exit. 
![Tray Menu](/images/tray_menu.png)

You can also stop connections from this menu or open the key manager window.

## Key Manager

![Key Manager](/images/key_manager.png)

The key manager allows you to manage your keys used to authenticate over spacebridge. If you don't have a local key it can create one to make authentication possible.
