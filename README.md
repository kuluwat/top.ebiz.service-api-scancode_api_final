# Ignore specific files
สามารถช่วยเพิ่มบรรทัดในไฟล์ `.gitignore` เพื่อไม่ให้ Git ติดตามไฟล์ `Properties/launchSettings.json` และ `web.config` ได้ไหม?

Properties/launchSettings.json
web.config

# Commands to remove files from Git tracking
git rm --cached Properties/launchSettings.json
git rm --cached web.config
git commit -m "ลบ launchSettings.json และ web.config ออกจากการติดตามของ Git"
git push

แล้วครั้งต่อไป
git add .
git commit -m "เพิ่ม comment ใน Readme.md และเพิ่ม Properties folder"
git push
# Ignore files in the root directory

 
