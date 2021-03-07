### Debian 10 Configuration ###

* `apt install apt-transport-https dirmngr`
* `apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF`
* `echo "deb https://download.mono-project.com/repo/debian stable-buster main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list`
* `apt update`
* `apt install mono-complete`
* `nano /etc/rc.local`
```
cd /root/ResourceBookingServer
mono Exe/ResourceBookingServer.exe 2>&1 >> log.save | logFormatter >> errors.save &
```