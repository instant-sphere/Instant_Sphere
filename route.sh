#!/system/bin/sh
ip delete default via 192.168.1.1 dev wlan0 proto static
ip route add `ip route show table rmnet0`
