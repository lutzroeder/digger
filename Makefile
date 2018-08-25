
publish:
	rm -rf ./build/gh-pages
	git clone git@github.com:lutzroeder/digger.git ./build/gh-pages --branch ./build/gh-pages
	rm -rf ./build/gh-pages/*
	cp ./index.html ./build/gh-pages/
	cp ./digger.js ./build/gh-pages/
	git -C ./build/gh-pages add --all
	git -C ./build/gh-pages commit --amend --no-edit
	git -C ./build/gh-pages push --force origin gh-pages
