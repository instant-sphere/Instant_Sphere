#!/system/bin/sh
ip route delete table wlan0 default via 192.168.1.1 dev wlan0 proto static
ip route add table wlan0 `ip route show table rmnet0`
ip route add table wlan0 `ip route show table rmnet1`
ip route add table wlan0 `ip route show table rmnet2`
ip route add table wlan0 `ip route show table rmnet3`
