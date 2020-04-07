# /bin/bash

KEYS=$(ls rotation | sort -V | tail -r)
set -- $KEYS
JUNIORKEY=${1//[^0-9]/}
NEXTKEY=$(($JUNIORKEY + 1))
#echo "keymaterial" > "rotation/pkey.$NEXTKEY.pem"
openssl genrsa \
 -out "rotation/pkey.$NEXTKEY.pem" 2048 > /dev/null 2>&1

DELETE=($KEYS)
for var in "${DELETE[@]:2}"
do
  rm "rotation/$var"
  # do something on $var
done

