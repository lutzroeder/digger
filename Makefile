
publish:
	rm -rf ./dist
	git clone git@github.com:lutzroeder/digger.git ./dist/gh-pages --branch gh-pages
	rm -rf ./dist/gh-pages/*
	cp ./index.html ./dist/gh-pages/
	git -C ./dist/gh-pages add --all
	git -C ./dist/gh-pages commit --amend --no-edit
	git -C ./dist/gh-pages push --force origin gh-pages
	rm -rf ./dist
